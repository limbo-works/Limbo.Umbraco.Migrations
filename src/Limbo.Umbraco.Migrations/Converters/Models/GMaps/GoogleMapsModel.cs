using System.Diagnostics.CodeAnalysis;
using Limbo.Umbraco.MigrationsClient.Models.Terratype;
using Newtonsoft.Json;

namespace Limbo.Umbraco.Migrations.Converters.Models.GMaps;

/// <summary>
/// Class representing a maps model from the <strong>Our.Umbraco.GMaps</strong> package.
/// </summary>
/// <see>
///     <cref>https://github.com/ArnoldV/Our.Umbraco.GMaps</cref>
/// </see>
public class GoogleMapsModel {

    [JsonProperty("address")]
    public GoogleMapsAddress Address { get; set; } = new();

    [JsonProperty("mapconfig")]
    public GoogleMapsConfig MapsConfig { get; set; } = new();

    [return: NotNullIfNotNull("terratype")]
    public static GoogleMapsModel? Convert(TerratypeModel? terratype) {

        if (terratype is null) return null;

        GoogleMapsModel maps = new() {
            Address = new GoogleMapsAddress {
                FullAddress = terratype.Lookup
            },
            MapsConfig = new GoogleMapsConfig {
                Zoom = terratype.Zoom ?? 17,
                MapType = GoogleMapsRoadType.Roadmap,
                Coordinates = new GoogleMapsPoint(terratype.Position.Latitude, terratype.Position.Longitude)
            }
        };

        return maps;

    }

}