using System;
using System.Collections.Generic;
using Limbo.Umbraco.Migrations.Constants;
using Limbo.Umbraco.MigrationsClient.Models.Content;
using Limbo.Umbraco.MigrationsClient.Models.Media;
using Newtonsoft.Json;
using Skybrud.Essentials.Json.Newtonsoft.Converters;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Limbo.Umbraco.Migrations.Models.Udis;

/// <summary>
/// Class representing a list of <see cref="GuidUdi"/> items.
/// </summary>
/// <remarks>When serialized to JSON using <strong>Newtonsoft.Json</strong>, this list will be serialized as a comma
/// separated string of UDIs.</remarks>
[JsonConverter(typeof(StringJsonConverter))]
public class GuidUdiList : List<GuidUdi> {

    public void AddContent(Guid key) {
        Add(new GuidUdi(UmbracoEntityTypes.Content, key));
    }

    public void AddContent(LegacyContent content) {
        Add(new GuidUdi(UmbracoEntityTypes.Content, content.Key));
    }

    public void AddContent(IContent content) {
        Add(content.GetUdi());
    }

    public void AddMedia(LegacyMedia media) {
        Add(new GuidUdi(UmbracoEntityTypes.Media, media.Key));
    }

    public void AddMedia(IMedia media) {
        Add(media.GetUdi());
    }

    public override string ToString() {
        return string.Join(",", this);
    }

}