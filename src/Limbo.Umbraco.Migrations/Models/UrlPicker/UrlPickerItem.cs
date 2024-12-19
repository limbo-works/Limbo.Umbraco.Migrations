using Newtonsoft.Json;
using Skybrud.Essentials.Json.Newtonsoft.Converters;
using Skybrud.Essentials.Json.Newtonsoft.Converters.Enums;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Limbo.Umbraco.Migrations.Models.UrlPicker;

public class UrlPickerItem {

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("target", NullValueHandling = NullValueHandling.Ignore)]
    public string? Target { get; set; }

    [JsonConverter(typeof(EnumCamelCaseConverter))]
    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    public LinkType Type { get; set; }

    [JsonConverter(typeof(StringJsonConverter))]
    [JsonProperty("udi", NullValueHandling = NullValueHandling.Ignore)]
    public Udi? Udi { get; set; }

    [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
    public string? Url { get; set; }

    [JsonProperty("queryString", NullValueHandling = NullValueHandling.Ignore)]
    public string? QueryString { get; set; }

    public UrlPickerItem(LinkType type, string name, Udi? udi, string? url, string? target) {
        Type = type;
        Name = name;
        Udi = udi;
        Url = url;
        Target = target;
    }

    public UrlPickerItem(LinkType type, string name, Udi? udi, string? url, string? target, string? queryString) {
        Type = type;
        Name = name;
        Udi = udi;
        Url = url;
        Target = target;
        QueryString = queryString;
    }

    public static UrlPickerItem CreateContentItem(string name, Udi udi, string? url, string? target) {
        return new UrlPickerItem(LinkType.Content, name, udi, url, target);
    }

    public static UrlPickerItem CreateContentItem(string name, Udi udi, string? url, string? target, string? queryString) {
        return new UrlPickerItem(LinkType.Content, name, udi, url, target, queryString);
    }

    public static UrlPickerItem CreateMediaItem(string name, Udi udi, string url, string? target) {
        return new UrlPickerItem(LinkType.Media, name, udi, url, target);
    }

    public static UrlPickerItem CreateMediaItem(IMedia media, string? url = null, string? target = null, string? queryString = null) {
        return new UrlPickerItem(LinkType.Media, media.Name ?? string.Empty, media.GetUdi(), url, target, queryString);
    }

    public static UrlPickerItem CreateMediaItem(string name, Udi udi, string url, string? target, string? queryString) {
        return new UrlPickerItem(LinkType.Media, name, udi, url, target, queryString);
    }

    public static UrlPickerItem CreateExternalItem(string name, string url, string? target) {
        return new UrlPickerItem(LinkType.External, name, null, url, target);
    }

    public static UrlPickerItem CreateExternalItem(string name, string url, string? target, string? queryString) {
        return new UrlPickerItem(LinkType.External, name, null, url, target, queryString);
    }

}