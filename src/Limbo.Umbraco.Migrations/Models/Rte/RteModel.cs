using Newtonsoft.Json;

namespace Limbo.Umbraco.Migrations.Models.Rte;

/// <summary>
/// Class representing the RTE model in Umbraco 13.
/// </summary>
public class RteModel {

    [JsonProperty("markup")]
    public string Markup { get; set; }

    [JsonProperty("blocks")]
    public RteBlocks Blocks { get; } = new();

    public RteModel(string markup) {
        Markup = markup;
    }

}