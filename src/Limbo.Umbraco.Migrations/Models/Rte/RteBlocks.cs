using System.Collections.Generic;
using Newtonsoft.Json;

namespace Limbo.Umbraco.Migrations.Models.Rte;

/// <summary>
/// Represents the blocks part of <see cref="RteModel"/>.
/// </summary>
public class RteBlocks {

    [JsonProperty("contentData")]
    public IReadOnlyList<object> ContentData { get; } = [];

    [JsonProperty("settingsData")]
    public IReadOnlyList<object> SettingsData { get; } = [];

}