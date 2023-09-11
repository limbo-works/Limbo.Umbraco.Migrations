using Newtonsoft.Json;

namespace Limbo.Umbraco.Migrations.Converters.Models.GMaps;

/// <summary>
/// Class representing an address model from the <strong>Our.Umbraco.GMaps</strong> package.
/// </summary>
/// <see>
///     <cref>https://github.com/ArnoldV/Our.Umbraco.GMaps</cref>
/// </see>
public class GoogleMapsAddress {

    [JsonProperty("full_address")]
    public string? FullAddress { get; set; }

    [JsonProperty("streetNumber", NullValueHandling = NullValueHandling.Ignore)]
    public string? StreetNumber { get; set; }

    [JsonProperty("street", NullValueHandling = NullValueHandling.Ignore)]
    public string? Street { get; set; }

    [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
    public string? State { get; set; }

    [JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
    public string? Country { get; set; }

    [JsonProperty("coordinates", NullValueHandling = NullValueHandling.Ignore)]
    public GoogleMapsPoint? Coordinates { get; set; }

}