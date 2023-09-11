using Newtonsoft.Json;
using Skybrud.Essentials.Json.Newtonsoft.Converters.Enums;

namespace Limbo.Umbraco.Migrations.Converters.Models.GMaps;

/// <summary>
/// Enum class representing the road type from the <strong>Our.Umbraco.GMaps</strong> package.
/// </summary>
/// <see>
///     <cref>https://github.com/ArnoldV/Our.Umbraco.GMaps</cref>
/// </see>
[JsonConverter(typeof(EnumLowerCaseConverter))]
public enum GoogleMapsRoadType {
    Roadmap,
    Hybrid
}