using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Limbo.Umbraco.Migrations.Constants;
using Limbo.Umbraco.Migrations.Exceptions;
using Limbo.Umbraco.Migrations.Models;
using Limbo.Umbraco.Migrations.Models.BlockList;
using Limbo.Umbraco.Migrations.Models.Content;
using Limbo.Umbraco.Migrations.Models.UrlPicker;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Content;
using Limbo.Umbraco.MigrationsClient.Models.ContentTypes;
using Limbo.Umbraco.MigrationsClient.Models.Media;
using Limbo.Umbraco.MigrationsClient.Models.Properties;
using Limbo.Umbraco.MigrationsClient.Models.Skybrud.LinkPicker;
using Limbo.Umbraco.MigrationsClient.Models.Umbraco;
using Limbo.Umbraco.MigrationsClient.Models.Umbraco.NestedContent;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Exceptions;
using Skybrud.Essentials.Http.Exceptions;
using Skybrud.Essentials.Json.Newtonsoft;
using Skybrud.Essentials.Json.Newtonsoft.Extensions;
using Skybrud.Essentials.Strings.Extensions;
using Skybrud.Umbraco.GridData.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using GuidUdi = Umbraco.Cms.Core.GuidUdi;

// ReSharper disable ReturnTypeCanBeNotNullable

namespace Limbo.Umbraco.Migrations.Services;

public partial class MigrationsServiceBase : IMigrationsService {

    #region Properties

    public MigrationsServiceDependencies Dependencies { get; }

    public IContentService ContentService => Dependencies.ContentService;

    public IMediaService MediaService => Dependencies.MediaService;

    public IMigrationsClient MigrationsClient => Dependencies.MigrationsClient;

    public int MigrationUserId { get; protected set; } = global::Umbraco.Cms.Core.Constants.Security.SuperUserId;

    protected HashSet<int> IgnoredIds { get; } = new();

    protected HashSet<Guid> IgnoredKeys { get; } = new();

    #endregion

    #region Constructors

    public MigrationsServiceBase(MigrationsServiceDependencies dependencies) {
        Dependencies = dependencies;
    }

    #endregion

    #region Member methods

    public virtual ContentImportResult ImportContent(int id) {
        return ImportContent(id, true);
    }

    public virtual ContentImportResult ImportContent(int id, bool update) {

        // Get the legacy content item from the old site
        LegacyContent source = MigrationsClient.GetContentById(id);

        // Check whether the content item already exists
        IContent? content = ContentService.GetById(source.Key);

        // Return right away if the update flag is false and the content node already exists
        if (!update && content is not null) return new ContentImportResult(ContentImportStatus.NotModified, content);

        return ImportContent(source, content, update);

    }

    public virtual ContentImportResult ImportContent(Guid key) {
        return ImportContent(key, true);
    }

    public virtual ContentImportResult ImportContent(Guid key, bool update) {

        // Check whether the content item already exists
        IContent? content = ContentService.GetById(key);

        // Return right away if the update flag is false and the content node already exists
        if (!update && content is not null) return new ContentImportResult(ContentImportStatus.NotModified, content);

        // Get the legacy content item from the old site
        LegacyContent source = MigrationsClient.GetContentByKey(key);

        return ImportContent(source, content, update);

    }

    public virtual ContentImportResult ImportContent(LegacyContent source, IContent? content, bool update) {

        try {

            // Convert the content
            MigrationsContentModel model;
            try {
                model = ConvertContent(source, content);
            } catch (Exception ex) {
                throw new MigrationsException($"Failed converting properties for page with key '{source.Key}'...", ex);
            }

            int parentId = -1;

            if (model.ParentKey is not null) {

                // Attempt to import the parent
                ContentImportResult result = ImportContent(model.ParentKey.Value, false);

                // Throw a new exception if importing the parent failed
                if (result.Status == ContentImportStatus.Failed) throw new MigrationsException($"Failed importing parent for '{source.Key}'...", result.Exception);

                parentId = result.Content!.Id;

            }

            ContentImportStatus status;

            if (content is null) {

                // Set the status
                status = ContentImportStatus.Created;

                // Create the new content item (in memory for now)
                content = ContentService.Create(model.Name, parentId, model.ContentTypeAlias, MigrationUserId);

                // Make sure we use the same GUID key
                content.Key = source.Key;

                // Update the properties
                UpdateProperties(model, content);

            } else {

                // Return right away if the updated flag is false
                if (!update) return new ContentImportResult(ContentImportStatus.NotModified, content);

                // Update the properties
                bool modified = UpdateProperties(model, content);

                // Set the status
                status = modified ? ContentImportStatus.Updated : ContentImportStatus.NotModified;

            }

            // Save and publish the content item
            if (status != ContentImportStatus.NotModified) ContentService.SaveAndPublish(content, userId: MigrationUserId);

            // Return the result
            return new ContentImportResult(status, content);

        } catch (Exception ex) {

            return new ContentImportResult(ex);

        }

    }

    public virtual IMedia? ImportMedia(int id) {

        if (IgnoredIds.Contains(id)) return null;

        // Get the legacy media item from the old site
        LegacyMedia source = MigrationsClient.GetMediaById(id);

        // Check whether the media item already exists
        IMedia? media = MediaService.GetById(source.Key);
        if (media is not null) return media;

        // Import the media
        return ImportMedia(source);

    }

    public virtual IMedia? ImportMedia(Guid key) {

        if (IgnoredKeys.Contains(key)) return null;

        // Check whether the media item already exists
        IMedia? media = MediaService.GetById(key);
        if (media is not null) return media;

        // Get the legacy media item from the old site
        LegacyMedia source = MigrationsClient.GetMediaByKey(key);

        // Import the media
        return ImportMedia(source);

    }

    protected virtual IMedia ImportMedia(LegacyMedia source) {

        // Determine the parent (it will be imported if it hasn't already been imported)
        var parent = source.Path.Count == 0 ? null : ImportMedia(source.Path.Last().Key);

        return source.ContentTypeAlias switch {
            "Folder" => ImportMediaFolder(source, parent),
            "Image" => ImportMediaImage(source, parent),
            "File" => ImportMediaFile(source, parent),
            "video" => ImportMediaFile(source, parent),
            _ => throw new Exception($"Unsupported media type: {source.ContentTypeAlias}\r\n\r\nID: {source.Id}\r\nKey: {source.Key}")
        };

    }

    protected virtual IMedia ImportMediaFolder(LegacyMedia source, IMedia? parent) {

        // Create the new media item (in memory for now)
        IMedia folder = MediaService.CreateMediaWithIdentity(source.Name, parent?.Id ?? -1, "Folder", MigrationUserId);

        // Make sure we use the same GUID key
        folder.Key = source.Key;
        folder.CreateDate = source.CreateDate.DateTimeOffset.DateTime;

        // Update custom properties
        UpdateProperties(source, folder);

        // Save the media to the database
        MediaService.Save(folder, MigrationUserId);

        // Return the media
        return folder;

    }

    protected virtual IMedia ImportMediaImage(LegacyMedia source, IMedia? parent) {

        string? umbracoFilePath = source.JObject.GetStringByPath("properties.umbracoFile.value.src") ?? source.JObject.GetStringByPath("properties.umbracoFile.value");
        source.JObject.TryGetDoubleByPath("properties.umbracoFile.value.focalPoint.left", out double? left);
        source.JObject.TryGetDoubleByPath("properties.umbracoFile.value.focalPoint.top", out double? top);

        if (string.IsNullOrWhiteSpace(umbracoFilePath)) throw new Exception($"Media with key {source.Key} and doesn't have a valid path.\r\n\r\n" + source.JObject);

        // Map the path to the TEMP dir
        string tempDir = Dependencies.WebHostEnvironment.MapPathContentRoot($"{(global::Umbraco.Cms.Core.Constants.SystemDirectories.TempData)}/Limbo.Umbraco.Migrations");

        string mediaPath = Path.Combine(tempDir, Guid.NewGuid().ToString());
        string filename = Path.GetFileName(umbracoFilePath);

        MigrationsClient.DownloadBytes(source, mediaPath);

        IMedia m = MediaService.CreateMediaWithIdentity(source.Name, parent?.Id ?? -1, source.ContentTypeAlias, MigrationUserId);
        m.Key = source.Key;
        m.CreateDate = source.CreateDate.DateTimeOffset.DateTime;

        Stream stream = System.IO.File.OpenRead(mediaPath);

        m.SetValue(
            Dependencies.MediaFileManager,
            Dependencies.MediaUrlGeneratorCollection,
            Dependencies.ShortStringHelper,
            Dependencies.ContentTypeBaseServiceProvider,
            global::Umbraco.Cms.Core.Constants.Conventions.Media.File,
            filename,
            stream
        );

        stream.Close();

        // For images, add the focal point to the "umbracoFile" property
        if (left is not null && top is not null) {
            string umbracoFileRaw = m.GetValue<string>(global::Umbraco.Cms.Core.Constants.Conventions.Media.File)!;
            if (umbracoFileRaw.StartsWith("/media/")) {
                var umb = new JObject {
                    {"src", umbracoFileRaw},
                    { "focalPoint", new JObject {{ "left",left.Value}, {"top",top.Value }}}
                };
                m.SetValue(global::Umbraco.Cms.Core.Constants.Conventions.Media.File, umb.ToString(Formatting.None));
            } else if (JsonUtils.TryParseJsonObject(umbracoFileRaw, out JObject? umbracoFile)) {
                umbracoFile.Add(new JObject {
                    { "focalPoint", new JObject {{ "left",left.Value}, {"top",top.Value }}}
                });
                m.SetValue(global::Umbraco.Cms.Core.Constants.Conventions.Media.File, umbracoFile.ToString(Formatting.None));
                throw new Exception(umbracoFile.ToString(Formatting.Indented));
            }
        }

        // Update custom properties
        UpdateProperties(source, m);

        // Save the media
        MediaService.Save(m, MigrationUserId);

        // Write the new Umbraco ID to the file
        System.IO.File.Delete(mediaPath);

        return m;

    }

    protected virtual IMedia ImportMediaFile(LegacyMedia source, IMedia? parent) {

        string? umbracoFilePath = source.JObject.GetStringByPath("properties.umbracoFile.value.src") ?? source.JObject.GetStringByPath("properties.umbracoFile.value");
        if (string.IsNullOrWhiteSpace(umbracoFilePath)) throw new Exception($"Media with key {source.Key} and doesn't have a valid path.\r\n\r\n" + source.JObject);

        // Map the path to the TEMP dir
        string tempDir = Dependencies.WebHostEnvironment.MapPathContentRoot($"{(global::Umbraco.Cms.Core.Constants.SystemDirectories.TempData)}/Limbo.Umbraco.Migrations");

        string mediaPath = Path.Combine(tempDir, Guid.NewGuid().ToString());
        string filename = Path.GetFileName(umbracoFilePath);
        string? extension = source.GetString("umbracoExtension");

        string contentTypeAlias = source.ContentTypeAlias switch {
            "video" => UmbracoMediaTypes.Video,
            _ => extension switch {
                "pdf" => UmbracoMediaTypes.Pdf,
                "svg" => UmbracoMediaTypes.Svg,
                _ => throw new MigrationsException($"Unknown file extension '{extension}' for media with key '{source.Key}'.")
            }
        };

        MigrationsClient.DownloadBytes(source, mediaPath);

        IMedia m = MediaService.CreateMediaWithIdentity(source.Name, parent?.Id ?? -1, contentTypeAlias, MigrationUserId);
        m.Key = source.Key;
        m.CreateDate = source.CreateDate.DateTimeOffset.DateTime;

        Stream stream = System.IO.File.OpenRead(mediaPath);

        m.SetValue(
            Dependencies.MediaFileManager,
            Dependencies.MediaUrlGeneratorCollection,
            Dependencies.ShortStringHelper,
            Dependencies.ContentTypeBaseServiceProvider,
            global::Umbraco.Cms.Core.Constants.Conventions.Media.File,
            filename,
            stream
        );

        stream.Close();

        // Update custom properties
        UpdateProperties(source, m);

        // Save the media
        MediaService.Save(m, MigrationUserId);

        // Write the new Umbraco ID to the file
        System.IO.File.Delete(mediaPath);

        return m;

    }

    /// <summary>
    /// Returns whether <paramref name="ex"/> or any of its inner exceptions represents a 404 HTTP response.
    /// </summary>
    /// <param name="ex">The exception.</param>
    /// <returns><see langword="true"/> if <paramref name="ex"/> or any of its inner exceptions represents a 404 HTTP response; otherwise, <see langword="false"/>.</returns>
    public virtual bool Is404(Exception ex) {

        Exception? scope = ex;

        while (scope is not null) {
            if (scope is HttpException { StatusCode: HttpStatusCode.NotFound }) return true;
            if (scope.Message.StartsWith("DreamBroker video with ID ") && scope.Message.EndsWith(" not found.")) return true;
            scope = scope.InnerException;
        }

        return false;

    }

    /// <summary>
    /// Returns the new content type of the specified <paramref name="entity"/>.
    ///
    /// By default, this method will return the existing content type alias, but it may be overriden to map existing aliases to new aliases.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The new content type alias for <paramref name="entity"/>.</returns>
    public virtual string GetContentTypeAlias(ILegacyElement entity) {
        return entity.ContentTypeAlias;
    }

    public virtual string GetPropertyAlias(ILegacyElement entity, ILegacyProperty property) {
        return property.Alias;
    }

    public virtual MigrationsContentModel ConvertContent(LegacyContent entity, IContentBase? content) {

        MigrationsContentModel model = new(entity) {
            ContentTypeAlias = GetContentTypeAlias(entity)
        };

        foreach (ILegacyProperty property in entity.Properties) {

            // Determine the new property alias (usually the same)
            string propertyAlias = GetPropertyAlias(entity, property);

            // Convert the property value
            object? newValue = ConvertPropertyValue(entity, property);

            // Update the property value
            model.SetValue(propertyAlias, newValue);

        }

        return model;

    }

    public virtual bool UpdateProperties(MigrationsContentModel model, IContentBase content) {

        bool modified = false;

        if (content.Name != model.Name) {
            content.Name = model.Name;
            modified = true;
        }

        foreach (var property in model.Properties) {

            object? current = content.GetValue(property.Key);

            switch (property.Value) {

                case null:
                case int:
                case string:
                case bool:
                    content.SetValue(property.Key, property.Value);
                    modified |= current != property.Value;
                    break;

                case DateTime dt:
                    content.SetValue(property.Key, dt);
                    modified |= current != property.Value;
                    break;

                default:
                    if (property.Value.GetType().FullName!.StartsWith("System.")) throw new Exception("WTF? " + property.Value.GetType() + " => " + property.Value);
                    string newValue = JToken.FromObject(property.Value).ToString(Formatting.None);
                    content.SetValue(property.Key, newValue);
                    modified |= !Equals(current, newValue);
                    break;

            }

        }

        return modified;

    }

    protected virtual void UpdateProperties(LegacyMedia source, IMedia media) {

        // we don't really do anything here, but allow the method to be overridden

    }

    protected virtual void ConvertProperties(ILegacyElement entity, IContentBase content) {

        // Convert the individual properties
        foreach (ILegacyProperty property in entity.Properties) {

            // Determine the new property alias (usually the same)
            string propertyAlias = GetPropertyAlias(entity, property);

            object? newValue = ConvertPropertyValue(entity, property);

            switch (newValue) {

                case null:
                    continue;

                case int numeric:
                    content.SetValue(propertyAlias, numeric);
                    break;

                case string str:
                    content.SetValue(propertyAlias, str);
                    break;

                case DateTime dt:
                    content.SetValue(propertyAlias, dt);
                    break;

                default:
                    if (newValue.GetType().FullName!.StartsWith("System.")) throw new Exception("WTF? " + newValue.GetType() + " => " + newValue);
                    content.SetValue(propertyAlias, JToken.FromObject(newValue).ToString(Formatting.None));
                    break;

            }

        }

    }

    public virtual object? ConvertPropertyValue(ILegacyElement owner, ILegacyProperty property) {

        // Look for a converter that knows how to convert the property value
        if (Dependencies.PropertyConverterCollection.FirstOrDefault(x => x.IsConverter(owner, property)) is { } converter) {
            try {
                return converter.Convert(owner, property);
            } catch (Exception ex) {
                throw new MigrationsConvertPropertyException(owner, property, $"Failed converting value of property with alias '{property.Alias}' on page with key {owner.Key}...", ex);
            }
        }

        StringBuilder sb = new();
        sb.AppendLine($"Unknown property editor alias: {property.EditorAlias}");
        sb.AppendLine("Content type: " + owner.ContentTypeAlias);
        sb.AppendLine("Property: " + property.Alias);
        sb.AppendLine("Value Type: " + (property.Value.GetType().FullName ?? "NULL"));
        sb.AppendLine("Token Type: " + property.Value.Type);
        sb.AppendLine();
        sb.AppendLine("--" + property.Value + "--");
        throw new Exception(sb.ToString());

    }

    public BlockListItem<TContent, TSettings> CreateBlockListItem<TContent, TSettings>(Guid contentKey, Guid settingsKey) where TContent : PublishedElementModel where TSettings : PublishedElementModel {
        BlockListContentData<TContent> content = CreateBlockListContentData<TContent>(contentKey);
        BlockListSettingsData<TSettings> settings = CreateBlockListSettingsData<TSettings>(settingsKey);
        return new BlockListItem<TContent, TSettings>(content, settings);
    }

    public virtual BlockListSettingsData? CreateDefaultBlockListSettings(GridControl control) {
        return null;
    }

    public virtual BlockListContentData<T> CreateBlockListContentData<T>(Guid key) where T : PublishedElementModel {
        Type type = typeof(BlockListContentData<>).MakeGenericType(typeof(T));
        return (BlockListContentData<T>) Activator.CreateInstance(type, key, GetModelType<T>())!;
    }

    public virtual BlockListContentData<T> CreateBlockListContentData<T>(GridControl control) where T : PublishedElementModel {
        Type type = typeof(BlockListContentData<>).MakeGenericType(typeof(T));
        return (BlockListContentData<T>) Activator.CreateInstance(type, control, GetModelType<T>())!;
    }

    public virtual BlockListContentData<T> CreateBlockListContentData<T>(MigrationsClient.Models.Skybrud.Grid.GridControl control) where T : PublishedElementModel {
        Type type = typeof(BlockListContentData<>).MakeGenericType(typeof(T));
        return (BlockListContentData<T>) Activator.CreateInstance(type, control, GetModelType<T>())!;
    }

    public virtual BlockListSettingsData<T> CreateBlockListSettingsData<T>(Guid key) where T : PublishedElementModel {
        Type type = typeof(BlockListSettingsData<>).MakeGenericType(typeof(T));
        return (BlockListSettingsData<T>) Activator.CreateInstance(type, key, GetModelType<T>())!;
    }

    public virtual BlockListSettingsData<T> CreateBlockListSettingsData<T>(GridControl control) where T : PublishedElementModel {
        Type type = typeof(BlockListSettingsData<>).MakeGenericType(typeof(T));
        return (BlockListSettingsData<T>) Activator.CreateInstance(type, control, GetModelType<T>())!;
    }

    public virtual BlockListSettingsData<T> CreateBlockListSettingsData<T>(MigrationsClient.Models.Skybrud.Grid.GridControl control) where T : PublishedElementModel {
        Type type = typeof(BlockListSettingsData<>).MakeGenericType(typeof(T));
        return (BlockListSettingsData<T>) Activator.CreateInstance(type, control, GetModelType<T>())!;
    }

    public static IPublishedContentType GetModelType<T>() where T : PublishedElementModel {
        var t = typeof(T);
        var field = t.GetField("ModelTypeAlias");
        string alias = (string) field!.GetValue(null)!;
        return StaticServiceProvider.Instance.GetRequiredService<IUmbracoContextAccessor>().GetRequiredUmbracoContext().Content!.GetContentType(alias)!;
    }

    public virtual BlockListModel ConvertNestedContentToBlockList(NestedContentModel nestedContent) {
        return ConvertNestedContentToBlockList(null, null, nestedContent);
    }

    public virtual BlockListModel ConvertNestedContentToBlockList(ILegacyElement? owner, ILegacyProperty? property,  NestedContentModel nestedContent) {

        BlockListModel blockList = new();

        foreach (NestedContentItem item in nestedContent) {

            // Get the referenced content type
            LegacyContentType contentType = MigrationsClient.GetContentTypeByAlias(item.ContentTypeAlias);

            // Wrap the item and content type as an element
            NestedContentElement element = new(item, contentType);

            // Convert the Nested Content item to a corresponding Block List item
            BlockListItem? blockListItem = ConvertNestedContentItemToBlockListItem(owner, property, element);
            if (blockListItem != null) blockList.AddItem(blockListItem);

        }

        return blockList;

    }

    public virtual BlockListItem? ConvertNestedContentItemToBlockListItem(ILegacyElement? owner, ILegacyProperty? property, NestedContentElement element) {

        // Get the alias of the new content type
        string contentTypeAlias = GetContentTypeAlias(element);

        // Get a reference to the published content type
        var contentType = Dependencies.UmbracoContext.Content?.GetContentType(contentTypeAlias);
        if (contentType is null) throw new MigrationsException($"Content type with alias '{contentTypeAlias}' not found.");

        // Initialize a new block list content part
        BlockListContentData content = new(element.Key, contentType);

        if (!element.HasProperty("name") && contentType.PropertyTypes.FirstOrDefault(x => x.Alias == "name") is not null) {
            content.Properties["name"] = element.Name;
        }

        foreach (ILegacyProperty p in element.Properties) {

            // Get the new content type alias
            string propertyAlias = GetPropertyAlias(element, p);

            // Convert the property value
            object? value = ConvertPropertyValue(element, p);

            // Set the property value (it not null)
            if (value is not null) content.Properties[propertyAlias] = value;

        }

        return new BlockListItem(content);

    }

    public virtual UrlPickerList? ConvertLinkPickerList(LinkPickerList? list) {

        if (list is null) return null;

        UrlPickerList temp = new();

        foreach (LinkPickerItem item in list.Items) {

            UrlPickerItem? urlItem = ConvertLinkPickerItem(item);
            if (urlItem is not null) temp.Add(urlItem);

        }

        return temp.Count == 0 ? null : temp;

    }

    public virtual UrlPickerItem? ConvertLinkPickerItem(LinkPickerItem? item) {

        if (item is null) return null;

        if (IgnoredIds.Contains(item.Id)) return null;

        UdiParser.TryParse(item.Udi, out GuidUdi? udi);

        if (udi is not null && IgnoredKeys.Contains(udi.Guid)) return null;

        string? anchor = item.JObject.GetString("anchor");
        string? target = item.Target == "_self" ? null : item.Target.NullIfWhiteSpace();

        switch (item.Type) {

            case LinkPickerType.Content:
                try {
                    if (udi is not null) return UrlPickerItem.CreateContentItem(item.Name, udi, item.Url!, target);
                    LegacyContent content = MigrationsClient.GetContentById(item.Id);
                    return UrlPickerItem.CreateContentItem(item.Name, new GuidUdi(global::Umbraco.Cms.Core.Constants.UdiEntityType.Document, content.Key), item.Url!, target, queryString: anchor);

                } catch (Exception ex) {
                    throw new Exception($"Failed getting content with ID {item.Id}...", ex);
                }

            case LinkPickerType.Media:
                IMedia? media = ImportMedia(item.Id);
                return media is null ? null : UrlPickerItem.CreateMediaItem(item.Name, media.GetUdi(), item.Url!, target, queryString: anchor);

            default:
                return string.IsNullOrWhiteSpace(item.Url) ? null : UrlPickerItem.CreateExternalItem(item.Name, item.Url, target, queryString: anchor);

        }

    }

    public virtual UrlPickerList? ConvertLinkPickerItemAsList(LinkPickerItem? item) {
        return ConvertLinkPickerItem(item) is { } result ? new UrlPickerList(result) : null;
    }

    public virtual UrlPickerList? ConvertMultiUrlPickerList(MultiUrlPickerList? list) {

        if (list is null) return null;

        UrlPickerList temp = [];

        foreach (MultiUrlPickerItem item in list) {

            UrlPickerItem? urlItem = ConvertMultiUrlPickerItem(item);
            if (urlItem is not null) temp.Add(urlItem);

        }

        return temp.Count == 0 ? null : temp;

    }

    public virtual UrlPickerItem? ConvertMultiUrlPickerItem(MultiUrlPickerItem? item) {

        if (item is null) return null;

        HashSet<string> knownProperties = ["name", "target", "url", "udi", "queryString"];

        foreach (JProperty property in item.JObject.Properties()) {
            if (knownProperties.Contains(property.Name)) continue;
            throw new MigrationsException($"Unknown URL picker item property '{property.Name}'...\r\n\r\n{item.JObject}");
        }

        if (item.Udi is not null) {
            switch (item.Udi.EntityType) {
                case UmbracoEntityTypes.Content:
                    return UrlPickerItem.CreateContentItem(item.Name, new GuidUdi(item.Udi.EntityType, item.Udi.Guid), null, target: item.Target, queryString: item.QueryString);
                case UmbracoEntityTypes.Media:
                    IMedia? media = ImportMedia(item.Udi.Guid);
                    return media is null ? null : UrlPickerItem.CreateMediaItem(media, target: item.Target, queryString: item.QueryString);
                default:
                    throw new MigrationsException($"Unsupported entity type '{item.Udi.EntityType}'...\r\n\r\n{item.JObject}");
            }
        }

        if (string.IsNullOrWhiteSpace(item.Url)) throw new BjernerSaysNoException($"WTF? URL picker item has no URL or UDI:\r\n\r\n{item.JObject}");

        return UrlPickerItem.CreateExternalItem(item.Name, item.Url, item.Target, item.QueryString);

    }

    #endregion

}