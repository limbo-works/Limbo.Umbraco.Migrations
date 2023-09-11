using Newtonsoft.Json;

namespace Limbo.Umbraco.Migrations.Converters.Models.GMaps;

/// <summary>
/// Class representing a configuration model from the <strong>Our.Umbraco.GMaps</strong> package.
/// </summary>
/// <see>
///     <cref>https://github.com/ArnoldV/Our.Umbraco.GMaps</cref>
/// </see>
public class GoogleMapsConfig {

    [JsonProperty("zoom")]
    public int Zoom { get; set; }

    [JsonProperty("maptype")]
    public GoogleMapsRoadType MapType { get; set; }

    [JsonProperty("centerCoordinates")]
    public GoogleMapsPoint? Coordinates { get; set; }

}