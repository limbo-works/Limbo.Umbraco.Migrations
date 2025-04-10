using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Limbo.Umbraco.MigrationsClient.Models.Content;
using Limbo.Umbraco.MigrationsClient.Models.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Limbo.Umbraco.Migrations.Models.Content;

public class MigrationsContentBase : ILegacyContent {

    #region Properties

    [JsonIgnore]
    public LegacyContent Content { get; }

    public int Id => Content.Id;

    public Guid Key => Content.Key;

    public IReadOnlyList<ILegacyContentItem> Path => Content.Path;

    public string Name => Content.Name;

    public string Url => Content.Url;

    public string ContentTypeAlias => Content.ContentTypeAlias;

    [JsonIgnore]
    public int? ParentId => Content.Path.LastOrDefault()?.Id;

    [JsonIgnore]
    public Guid? ParentKey => Content.Path.LastOrDefault()?.Key;

    public IReadOnlyList<ILegacyProperty> Properties => Content.Properties;

    public IReadOnlyList<ILegacyContentItem> Children => Content.Children;

    #endregion

    #region Constructors

    public MigrationsContentBase(LegacyContent content) {
        Content = content;
    }

    #endregion

    #region Member methods

    public JToken? GetValue(string alias) {
        return Content.GetValue(alias);
    }

    public ILegacyProperty? GetProperty(string alias) {
        return Content.GetProperty(alias);
    }

    public bool HasProperty(string alias) {
        return Content.HasProperty(alias);
    }

    public bool TryGetValue(string alias, [NotNullWhen(true)] out JToken? result) {
        return Content.TryGetValue(alias, out result);
    }

    public bool TryGetProperty(string alias, [NotNullWhen(true)] out ILegacyProperty? result) {
        return Content.TryGetProperty(alias, out result);
    }

    #endregion

}