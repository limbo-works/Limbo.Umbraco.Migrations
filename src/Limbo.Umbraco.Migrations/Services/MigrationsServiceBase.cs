using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using Limbo.Umbraco.Migrations.Exceptions;
using Limbo.Umbraco.Migrations.Models.BlockList;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Newtonsoft;
using Skybrud.Essentials.Json.Newtonsoft.Extensions;
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

    public class MigrationsServiceBase : IMigrationsService {

        #region Properties

        public MigrationsServiceDependencies Dependencies { get; }

        public IContentService ContentService => Dependencies.ContentService;

        public IMediaService MediaService => Dependencies.MediaService;

        public IMigrationsHttpClient MigrationsHttpClient => Dependencies.MigrationsHttpClient;

        public int MigrationUserId { get; protected set; } = Constants.Security.SuperUserId;

        #endregion

        #region Constructors

        public MigrationsServiceBase(MigrationsServiceDependencies dependencies) {
            Dependencies = dependencies;
        }

        #endregion

        #region Member methods

        public virtual IContent? ImportContent(int id) {

            // Get the legacy content item from the old site
            LegacyContent source = MigrationsHttpClient.GetContentById(id).Body;

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
            LegacyContent source = MigrationsHttpClient.GetContentByKey(key).Body;

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

            // Convert the individual properties
            ConvertProperties(source, content);

            // Save and publish the content item
            ContentService.SaveAndPublish(content, userId: MigrationUserId);

            return content;

        }

        public virtual IMedia? ImportMedia(int id) {

            // Get the legacy media item from the old site
            LegacyMedia source = MigrationsHttpClient.GetMediaById(id).Body;

            // Check whether the media item already exists
            IMedia? media = MediaService.GetById(source.Key);
            if (media is not null) return media;

            // Import the media
            return ImportMedia(source);

        }

        public virtual IMedia? ImportMedia(Guid key) {

            // Check whether the media item already exists
            IMedia? media = MediaService.GetById(key);
            if (media is not null) return media;

            // Get the legacy media item from the old site
            LegacyMedia source = MigrationsHttpClient.GetMediaByKey(key).Body;

            // Import the media
            return ImportMedia(source);

        }

        protected virtual IMedia ImportMedia(LegacyMedia source) {

            // Determine the parent (it will be imported if it hasn't already been imported)
            var parent = source.Path.Count == 0 ? null : ImportMedia(source.Path.Last().Key);

            return source.Type switch {
                "Folder" => ImportMediaFolder(source, parent),
                "Image" => ImportMediaImage(source, parent),
                "File" => ImportMediaFile(source, parent),
                _ => throw new Exception($"Unsupported media type: {source.Type}")
            };

        }

        protected virtual IMedia ImportMediaFolder(LegacyMedia source, IMedia? parent) {

            // Create the new media item (in memory for now)
            IMedia folder = MediaService.CreateMediaWithIdentity(source.Name, parent?.Id ?? -1, "Folder", MigrationUserId);

            // Make sure we use the same GUID key
            folder.Key = source.Key;

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
            string tempDir = Dependencies.WebHostEnvironment.MapPathContentRoot($"{Constants.SystemDirectories.TempData}/Limbo.Umbraco.Migrations");

            string mediaPath = Path.Combine(tempDir, Guid.NewGuid().ToString());
            string filename = Path.GetFileName(umbracoFilePath);

            MigrationsHttpClient.DownloadBytes(source, mediaPath);

            IMedia m = MediaService.CreateMediaWithIdentity(source.Name, parent?.Id ?? -1, source.Type, MigrationUserId);
            m.Key = source.Key;

            Stream stream = System.IO.File.OpenRead(mediaPath);

            m.SetValue(
                Dependencies.MediaFileManager,
                Dependencies.MediaUrlGeneratorCollection,
                Dependencies.ShortStringHelper,
                Dependencies.ContentTypeBaseServiceProvider,
                Constants.Conventions.Media.File,
                filename,
                stream
            );

            stream.Close();

            // For images, add the focal point to the "umbracoFile" property
            if (left is not null && top is not null) {
                string umbracoFileRaw = m.GetValue<string>(Constants.Conventions.Media.File)!;
                if (umbracoFileRaw.StartsWith("/media/")) {
                    var umb = new JObject {
                        {"src", umbracoFileRaw},
                        { "focalPoint", new JObject {{ "left",left.Value}, {"top",top.Value }}}
                    };
                    m.SetValue(Constants.Conventions.Media.File, umb.ToString(Formatting.None));
                } else if (JsonUtils.TryParseJsonObject(umbracoFileRaw, out JObject? umbracoFile)) {
                    umbracoFile.Add(new JObject {
                        { "focalPoint", new JObject {{ "left",left.Value}, {"top",top.Value }}}
                    });
                    m.SetValue(Constants.Conventions.Media.File, umbracoFile.ToString(Formatting.None));
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
            string tempDir = Dependencies.WebHostEnvironment.MapPathContentRoot($"{Constants.SystemDirectories.TempData}/Limbo.Umbraco.Migrations");

            string mediaPath = Path.Combine(tempDir, Guid.NewGuid().ToString());
            string filename = Path.GetFileName(umbracoFilePath);

            MigrationsHttpClient.DownloadBytes(source, mediaPath);

            IMedia m = MediaService.CreateMediaWithIdentity(source.Name, parent?.Id ?? -1, source.Type, MigrationUserId);
            m.Key = source.Key;

            Stream stream = System.IO.File.OpenRead(mediaPath);

            m.SetValue(
                Dependencies.MediaFileManager,
                Dependencies.MediaUrlGeneratorCollection,
                Dependencies.ShortStringHelper,
                Dependencies.ContentTypeBaseServiceProvider,
                Constants.Conventions.Media.File,
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
        public virtual string GetContentTypeAlias(LegacyEntity entity) {
            return entity.Type;
        }

        public virtual string GetPropertyAlias(LegacyEntity entity, LegacyProperty property) {
            return property.Alias;
        }

        protected virtual void ConvertProperties(LegacyEntity entity, IContentBase content) {

            // Convert the individual properties
            foreach (LegacyProperty property in entity.Properties) {

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

        public virtual object? ConvertPropertyValue(LegacyEntity owner, LegacyProperty property) {

            // Look for a converter that knows how to convert the property value
            if (Dependencies.PropertyConverterCollection.FirstOrDefault(x => x.IsConverter(property)) is { } converter) {
                try {
                    return converter.Convert(owner, property);
                } catch (Exception ex) {
                    throw new MigrationsConvertPropertyException(owner, property, $"Failed converting value of property with alias '{property.Alias}' on page with ID {owner.Id}...", ex);
                }
            }

            StringBuilder sb = new();
            sb.AppendLine($"Unknown property editor alias: {property.EditorAlias}");
            sb.AppendLine("Content type: " + owner.Type);
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

        public virtual BlockListContentData CreateBlockListContentData<T>(Guid key) where T : PublishedElementModel {
            return new BlockListContentData(key, GetModelType<T>());
        }

        public virtual BlockListContentData CreateBlockListContentData<T>(GridControl control) where T : PublishedElementModel {
            return new BlockListContentData(control, GetModelType<T>());
        }

        public virtual BlockListSettingsData CreateBlockListSettingsData<T>(GridControl control) where T : PublishedElementModel {
            return new BlockListSettingsData(control, GetModelType<T>());
        }

        public static IPublishedContentType GetModelType<T>() where T : PublishedElementModel {
            var t = typeof(T);
            var field = t.GetField("ModelTypeAlias");
            string alias = (string) field!.GetValue(null)!;
            return StaticServiceProvider.Instance.GetRequiredService<IUmbracoContextAccessor>().GetRequiredUmbracoContext().Content!.GetContentType(alias)!;
        }

        public virtual bool TryParseUdi(string value, [NotNullWhen(true)] out GuidUdi? result) {
            bool _ = UdiParser.TryParse(value, out Udi? udi);
            result = udi as GuidUdi;
            return result is not null;
        }

        public virtual string? ConvertRte(string source) {

            if (source.Contains("umb://media/")) {

                HtmlDocument document = new();
                document.LoadHtml(source);

                var links = document.DocumentNode.Descendants("a");
                var images = document.DocumentNode.Descendants("img");

                bool modified = false;

                if (links is not null) {

                    foreach (var link in links) {

                        string dataUdi = link.GetAttributeValue("data-udi", "");

                        if (!dataUdi.StartsWith("umb://media/")) continue;

                        Guid mediaKey = Guid.Parse(dataUdi[12..]);

                        IMedia? media = ImportMedia(mediaKey);
                        if (media is null) continue;

                        string? mediaUrl = media.GetValue<string>("umbracoFile");

                        if (string.IsNullOrWhiteSpace(mediaUrl)) {
                            throw new Exception("Media doesn't specify a valid file path.");
                        }
                        if (JsonUtils.TryParseJsonObject(mediaUrl, out JObject? mediaUrlJson)) {
                            mediaUrl = mediaUrlJson.GetString("src")!;
                        }

                        if (!mediaUrl.StartsWith("/media/")) throw new Exception("Not sure how to parse \"umbracoFile\" property value.\r\n\r\n" + mediaUrl);

                        link.SetAttributeValue("href", mediaUrl);
                        modified = true;

                    }
                }

                if (images is not null) {

                    foreach (HtmlNode node in images) {

                        string dataUdi = node.GetAttributeValue("data-udi", "");
                        if (!dataUdi.StartsWith("umb://media/")) continue;

                        Guid mediaKey = Guid.Parse(dataUdi[12..]);

                        IMedia? media = ImportMedia(mediaKey);
                        if (media is null) continue;

                        string? mediaUrl = media.GetValue<string>("umbracoFile");

                        if (string.IsNullOrWhiteSpace(mediaUrl)) {
                            throw new Exception("Media doesn't specify a valid file path.");
                        }

                        if (JsonUtils.TryParseJsonObject(mediaUrl, out JObject? mediaUrlJson)) {
                            mediaUrl = mediaUrlJson.GetString("src");
                        }

                        if (mediaUrl is null || !mediaUrl.StartsWith("/media/")) throw new Exception("Not sure hot to parse \"umbracoFile\" property value.\r\n\r\n" + mediaUrl);

                        node.SetAttributeValue("src", mediaUrl);
                        modified = true;

                    }
                }

                if (modified) {
                    source = document.DocumentNode.OuterHtml;
                }

            }

            return source;

        }


        #endregion

    }

}