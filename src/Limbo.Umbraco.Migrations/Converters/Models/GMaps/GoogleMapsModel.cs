using System.Diagnostics.CodeAnalysis;
using Limbo.Umbraco.MigrationsClient.Models.Terraform;
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

    [return: NotNullIfNotNull("terraform")]
    public static GoogleMapsModel? Convert(TerraformModel? terraform) {

        if (terraform is null) return null;

        GoogleMapsModel maps = new() {
            Address = new GoogleMapsAddress {
                FullAddress = terraform.Lookup
            },
            MapsConfig = new GoogleMapsConfig {
                Zoom = terraform.Zoom ?? 17,
                MapType = GoogleMapsRoadType.Roadmap,
                Coordinates = new GoogleMapsPoint(terraform.Position.Latitude, terraform.Position.Longitude)
            }
        };

        return maps;

    }

}