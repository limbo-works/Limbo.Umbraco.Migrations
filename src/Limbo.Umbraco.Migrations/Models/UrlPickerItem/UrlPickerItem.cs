using Newtonsoft.Json;
using Skybrud.Essentials.Json.Newtonsoft.Converters;
using Umbraco.Cms.Core;

namespace Limbo.Umbraco.Migrations.Models.UrlPickerItem {

    public class UrlPickerItem {

        [JsonProperty("name")]
        public string Name { get; }

        [JsonConverter(typeof(StringJsonConverter))]
        [JsonProperty("udi", NullValueHandling = NullValueHandling.Ignore)]
        public GuidUdi? Udi { get; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string? Url { get; }

        [JsonProperty("target", NullValueHandling = NullValueHandling.Ignore)]
        public string? Target { get; }

        public UrlPickerItem(string name, GuidUdi? udi, string? url, string? target) {
            Name = name;
            Udi = udi;
            Url = url;
            Target = target;
        }

    }

}