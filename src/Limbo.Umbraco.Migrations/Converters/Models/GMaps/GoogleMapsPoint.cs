using Newtonsoft.Json;

namespace Limbo.Umbraco.Migrations.Converters.Models.GMaps;

/// <summary>
/// Class representing a point from the <strong>Our.Umbraco.GMaps</strong> package.
/// </summary>
/// <see>
///     <cref>https://github.com/ArnoldV/Our.Umbraco.GMaps</cref>
/// </see>
public class GoogleMapsPoint {

    [JsonProperty("lat")]
    public double Latitude { get; set; }

    [JsonProperty("lng")]
    public double Longitude { get; set; }

    public GoogleMapsPoint(double latitude, double longitude) {
        Latitude = latitude;
        Longitude = longitude;
    }

}