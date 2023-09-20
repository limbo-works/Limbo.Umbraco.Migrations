using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Limbo.Umbraco.Migrations.Exceptions;
using Limbo.Umbraco.Migrations.Models.BlockList;
using Limbo.Umbraco.Migrations.Models.UrlPickerItem;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Content;
using Limbo.Umbraco.MigrationsClient.Models.ContentTypes;
using Limbo.Umbraco.MigrationsClient.Models.Media;
using Limbo.Umbraco.MigrationsClient.Models.Properties;
using Limbo.Umbraco.MigrationsClient.Models.Skybrud.LinkPicker;
using Limbo.Umbraco.MigrationsClient.Models.Umbraco.NestedContent;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Newtonsoft;
using Skybrud.Essentials.Json.Newtonsoft.Extensions;
using Skybrud.Essentials.Strings.Extensions;
using Skybrud.Umbraco.GridData.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

// ReSharper disable ReturnTypeCanBeNotNullable

namespace Limbo.Umbraco.Migrations.Services {

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

        public virtual IContent? ImportContent(int id) {

            // Get the legacy content item from the old site
            LegacyContent source = MigrationsClient.GetContentById(id);

            // Check whether the content item already exists
            IContent? content = ContentService.GetById(source.Key);
            if (content is not null) return content;

            // Import the content
            return ImportContent(source);

        }

        public virtual IContent? ImportContent(Guid key) {

            // Check whether the content item already exists
            IContent? content = ContentService.GetById(key);
            if (content is not null) return content;

            // Get the legacy content item from the old site
            LegacyContent source = MigrationsClient.GetContentByKey(key);

            return ImportContent(source);

        }

        protected virtual IContent ImportContent(LegacyContent source) {

            // Determine the parent (it will be imported if it hasn't already been imported)
            IContent? parent = source.Path.Count == 0 ? null : ImportContent(source.Path.Last().Key);

            // Determine the content type alias of the item to be created
            string contentTypeAlias = GetContentTypeAlias(source);

            // Create the new content item (in memory for now)
            IContent content = ContentService.Create(source.Name, parent?.Id ?? -1, contentTypeAlias, MigrationUserId);

            // Make sure we use the same GUID key
            content.Key = source.Key;
            content.CreateDate = source.CreateDate.DateTimeOffset.DateTime;

            // Convert the individual properties
            ConvertProperties(source, content);

            // Save and publish the content item
            ContentService.SaveAndPublish(content, userId: MigrationUserId);

            return content;

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
                _ => throw new Exception($"Unsupported media type: {source.ContentTypeAlias}")
            };

        }

        protected virtual IMedia ImportMediaFolder(LegacyMedia source, IMedia? parent) {

            // Create the new media item (in memory for now)
            IMedia folder = MediaService.CreateMediaWithIdentity(source.Name, parent?.Id ?? -1, "Folder", MigrationUserId);

            // Make sure we use the same GUID key
            folder.Key = source.Key;
            folder.CreateDate = source.CreateDate.DateTimeOffset.DateTime;

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

            // Save the media
            MediaService.Save(m, MigrationUserId);

            // Write the new Umbraco ID to the file
            System.IO.File.Delete(mediaPath);

            return m;

        }

        /// <summary>
        /// Returns the new content type of the specified <paramref name="entity"/>.
        ///
        /// By default this method will return the existing content type alias, but it may be overriden to map existing aliases to new aliases.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The new content type alias for <paramref name="entity"/>.</returns>
        public virtual string GetContentTypeAlias(ILegacyElement entity) {
            return entity.ContentTypeAlias;
        }

        public virtual string GetPropertyAlias(ILegacyElement entity, ILegacyProperty property) {
            return property.Alias;
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

        public virtual BlockListSettingsData<T> CreateBlockListSettingsData<T>(Guid key) where T : PublishedElementModel {
            Type type = typeof(BlockListSettingsData<>).MakeGenericType(typeof(T));
            return (BlockListSettingsData<T>) Activator.CreateInstance(type, key, GetModelType<T>())!;
        }

        public virtual BlockListSettingsData<T> CreateBlockListSettingsData<T>(GridControl control) where T : PublishedElementModel {
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

            string? target = item.Target == "_self" ? null : item.Target.NullIfWhiteSpace();

            switch (item.Mode) {

                case LinkPickerMode.Content:
                    try {
                        LegacyContent content = MigrationsClient.GetContentById(item.Id);
                        return UrlPickerItem.CreateContentItem(item.Name, new GuidUdi(global::Umbraco.Cms.Core.Constants.UdiEntityType.Document, content.Key), item.Url, target);
                    } catch (Exception ex) {
                        throw new Exception($"Failed getting content with ID {item.Id}...", ex);
                    }

                case LinkPickerMode.Media:
                    IMedia? media = ImportMedia(item.Id);
                    return media is null ? null : UrlPickerItem.CreateMediaItem(item.Name, media.GetUdi(), item.Url, target);

                default:
                    return string.IsNullOrWhiteSpace(item.Url) ? null : UrlPickerItem.CreateExternalItem(item.Name, item.Url, target);

            }

        }

        public virtual UrlPickerList? ConvertLinkPickerItemAsList(LinkPickerItem? item) {
            return ConvertLinkPickerItem(item) is { } result ? new UrlPickerList(result) : null;
        }

        #endregion

    }

}