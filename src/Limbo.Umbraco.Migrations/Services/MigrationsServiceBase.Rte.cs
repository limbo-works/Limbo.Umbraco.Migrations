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
using Limbo.Umbraco.Migrations.Models;
using Limbo.Umbraco.Migrations.Models.Rte;
using Limbo.Umbraco.MigrationsClient.Models.Media;

namespace Limbo.Umbraco.Migrations.Services;

public partial class MigrationsServiceBase {

    /// <summary>
    /// Converts the specified RTE <paramref name="input"/> string.
    /// </summary>
    /// <param name="input">The RTE input string to be converted.</param>
    /// <param name="warnings">When this method returns, holds a list of warnings.</param>
    /// <returns>An instance of <see cref="string"/> representing the reuslt of the conversion.</returns>
    public virtual RteModel? ConvertRte(string? input, out IReadOnlyList<Warning> warnings) {

        List<Warning> warningsList = [];
        warnings = warningsList;

        // Return null right away if input is null or white space
        if (string.IsNullOrWhiteSpace(input)) return null;

        // Parse the HTML
        HtmlDocument document = new();
        document.LoadHtml(input);

        // Create a flag to indicate whether the HTML was modified
        bool modified = false;

        // Convert images and links
        ConvertRteImages(document.DocumentNode, warningsList, ref modified);
        ConvertRteLinks(document.DocumentNode, warningsList, ref modified);

        // If the HTML was modified, we convert it back to a string - otherwise we return "input" directly
        return new RteModel(modified ? document.DocumentNode.OuterHtml : input);

    }

    protected virtual void ConvertRteLinks(HtmlNode root, List<Warning> warnings, ref bool modified) {

        IEnumerable<HtmlNode>? anchorLinks = root.Descendants("a");
        if (anchorLinks is null) return;

        foreach (HtmlNode link in anchorLinks) {
            ConvertRteLink(link, warnings, ref modified);
        }

    }

    protected virtual void ConvertRteLink(HtmlNode link, List<Warning> warnings, ref bool modified) {

        string href = link.GetAttributeValue("href", "");
        string dataUdi = link.GetAttributeValue("data-udi", "");

        // If the "href" attribute is a UDI reference to a media, we import that media. If the referenced is already in
        // the "href" attribute, and not a data attribute, we don't need to modify the element as it's already in the
        // correct format. Also notice that the REGEX doesn't match until the end of the line. This is because
        // "localLink" references may also include a fragment part (#)
        if (RegexUtils.IsMatch(href, @"^\/{localLink:(umb:\/\/media\/([a-z0-9]{32}))", out string udiRaw)) {
            if (UdiParser.TryParse(udiRaw, out GuidUdi? mediaUdi)) {
                try {
                    ImportMedia(mediaUdi.Guid);
                } catch (Exception ex) when (Is404(ex)) {
                    warnings.Add(new Warning($"Media with key '{mediaUdi.Guid}' not found.", ex));
                }
                return;
            }
        }

        // Older Umbraco sites may use the "localLink" syntax, but with a numeric ID. This has been used to link to
        // content (not media), and as such we can look up the reference content item via the migrations client, and
        // then update the "localLink" syntax with the content item's UDI instead.
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

        // Older Umbraco sites may specify the UDI via a data attribute, in which case we remove the "data-udi"
        // attribute and set/update the "href" attribute instead. Also, if the UDI reference is for a media, we make
        // sure to import said media
        if (UdiParser.TryParse(dataUdi, out GuidUdi? udi)) {

            // Remove the legacy attribute
            link.Attributes["data-udi"].Remove();

            // If UDI reference a media, we import that media
            if (udi.EntityType == UmbracoEntityTypes.Media) {
                try {
                    ImportMedia(udi.Guid);
                } catch (Exception ex) when (Is404(ex)) {
                    warnings.Add(new Warning($"Media with key '{udi.Guid}' not found.", ex));
                }
            }

            // Update the "href" attribute
            link.SetAttributeValue("href", $"/{{localLink:{udi}}}");

            modified = true;

        }

    }

    protected virtual void ConvertRteImages(HtmlNode root, List<Warning> warnings, ref bool modified) {

        IEnumerable<HtmlNode>? images = root.Descendants("img");
        if (images is null) return;

        foreach (HtmlNode img in images) {
            ConvertRteImage(img, warnings, ref modified);
        }

    }

    protected virtual void ConvertRteImage(HtmlNode img, List<Warning> warnings, ref bool modified) {

        string src = img.GetAttributeValue("src", "").Trim();
        string dataUdi = img.GetAttributeValue("data-udi", "").Trim();

        if (UdiParser.TryParse(dataUdi, out GuidUdi? udi)) {

            switch (udi.EntityType) {

                case UmbracoEntityTypes.Media: {

                    // Import the referenced media
                    IMedia? media;

                    try {
                        media = ImportMedia(udi.Guid);
                        if (media is null) return;
                    } catch (Exception ex) when(Is404(ex)) {
                        warnings.Add(new Warning($"Media with key '{udi.Guid}' not found.", ex));
                        return;
                    }

                    // Try to get the relative path to the media file (aka the URL)
                    if (!media.TryGetMediaPath("umbracoFile", Dependencies.MediaUrlGeneratorCollection, out string? mediaFilePath)) {
                        throw new MigrationsException($"Failed determining relative URL for media with key '{media.Key}'...");
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

        // Handle local media path references
        if (src.StartsWith("/media/")) {
            string path = src.Split('?')[0];
            try {
                LegacyMedia media = MigrationsClient.GetMediaByPath(path);
                ImportMedia(media.Key);
            } catch (Exception ex) when (Is404(ex)) {
                warnings.Add(new Warning($"Media with path '{path}' not found.", ex));
            }
            return;
        }

        // Handle "external" media path references
        if (src.Contains("/media/")) {
            throw new MigrationsException($"Unexpected media URL '{src}' in RTE value.");
        }

    }


}