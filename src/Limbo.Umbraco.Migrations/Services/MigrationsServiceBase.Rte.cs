using Limbo.Umbraco.Migrations.Exceptions;
using Limbo.Umbraco.MigrationsClient.Models.Content;
using Skybrud.Essentials.Strings;
using System;
using HtmlAgilityPack;
using Limbo.Umbraco.Migrations.Constants;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core;
using Umbraco.Extensions;
using System.Collections.Generic;

namespace Limbo.Umbraco.Migrations.Services {

    public partial class MigrationsServiceBase {

        /// <summary>
        /// Converts the specified RTE <paramref name="input"/> string.
        /// </summary>
        /// <param name="input">The RTE input string to be converted.</param>
        /// <returns>An instance of <see cref="string"/> representing the reuslt of the conversion.</returns>
        public virtual string? ConvertRte(string? input) {

            // Return null right away if input is null or white space
            if (string.IsNullOrWhiteSpace(input)) return null;

            // Parse the HTML
            HtmlDocument document = new();
            document.LoadHtml(input);

            // Create a flag to indicate whether the HTML was modified
            bool modified = false;

            // Convert images and links
            ConvertRteImages(document.DocumentNode, ref modified);
            ConvertRteLinks(document.DocumentNode, ref modified);

            // If the HTML was modified, we convert it back to a string - otherwise we return "input" directly
            return modified ? document.DocumentNode.OuterHtml : input;

        }

        protected virtual void ConvertRteLinks(HtmlNode root, ref bool modified) {

            IEnumerable<HtmlNode>? anchorLinks = root.Descendants("a");
            if (anchorLinks is null) return;

            foreach (HtmlNode link in anchorLinks) {
                ConvertRteLink(link, ref modified);
            }

        }

        protected virtual void ConvertRteLink(HtmlNode link, ref bool modified) {

            string href = link.GetAttributeValue("href", "");
            string dataUdi = link.GetAttributeValue("data-udi", "");

            if (RegexUtils.IsMatch(href, "/{localLink:([0-9]+)}", out int id)) {

                // Skip if ignored (eg. if trashed)
                if (IgnoredIds.Contains(id)) return;

                LegacyContent content;
                try {
                    content = MigrationsClient.GetContentById(id);
                } catch (Exception ex) {
                    throw new MigrationsException($"Failed getting content with ID {id}...", ex);
                }

                link.SetAttributeValue("href", $"/{{localLink:umb://document/{content.Key:N}}}");
                link.Attributes["data-id"]?.Remove();

                modified = true;
                return;

            }

            if (UdiParser.TryParse(dataUdi, out GuidUdi? udi) && udi is not null) {

                // Remove the legacy attribute
                link.Attributes["data-udi"].Remove();

                // If UDI reference a media, we import that media
                if (udi.EntityType == UmbracoEntityTypes.Media) ImportMedia(udi.Guid);

                // Update the "href" attribute
                link.SetAttributeValue("href", $"/{{localLink:{udi}}}");

                modified = true;

            }

        }

        protected virtual void ConvertRteImages(HtmlNode root, ref bool modified) {

            IEnumerable<HtmlNode>? images = root.Descendants("img");
            if (images is null) return;

            foreach (HtmlNode img in images) {
                ConvertRteImage(img, ref modified);
            }

        }

        protected virtual void ConvertRteImage(HtmlNode img, ref bool modified) {

            string src = img.GetAttributeValue("src", "");
            string dataUdi = img.GetAttributeValue("data-udi", "");

            if (UdiParser.TryParse(dataUdi, out GuidUdi? udi) && udi is not null) {

                switch (udi.EntityType) {

                    case UmbracoEntityTypes.Media: {

                            // Import the referenced media
                            IMedia? media = ImportMedia(udi.Guid);
                            if (media is null) break;

                            // Try to get the relative path to the media file (aka the URL)
                            if (!media.TryGetMediaPath("umbracoFile", Dependencies.MediaUrlGeneratorCollection, out string? mediaFilePath)) {
                                throw new Exception("Oh noes!");
                            }

                            // If the value of the "src" attribute is different from the media's current URL, we should set the new URL instead
                            if (src != mediaFilePath) {
                                img.SetAttributeValue("src", mediaFilePath);
                                modified = true;
                            }

                            return;

                        }

                    // An <img> element really should refer to anything other than media, so if we encounter this, we throw an exception
                    default:
                        throw new Exception($"Unsupported entity type: {udi.EntityType}");

                }

            }

            throw new Exception("Found unhandled <img /> element\r\n\r\n" + img.OuterHtml + "\r\n\r\n");

        }


    }

}