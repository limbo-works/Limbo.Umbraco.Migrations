using System.Collections.Generic;
using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Properties;
using Skybrud.Essentials.Strings.Extensions;

namespace Limbo.Umbraco.Migrations.Converters.Properties;

public class SkybrudTextBoxConverter : PropertyConverterBase {

    private readonly IReadOnlySet<string> _editorAliases = new HashSet<string> {
        "Skybrud.CharLimitEditor",
        "Skybrud.TextBox"
    };

    public SkybrudTextBoxConverter(IMigrationsService migrationsService, IMigrationsClient migrationsClient) : base(migrationsService, migrationsClient) { }

    public override bool IsConverter(ILegacyElement owner, ILegacyProperty property) {
        return _editorAliases.Contains(property.EditorAlias);
    }

    public override object? Convert(ILegacyElement owner, ILegacyProperty property) {
        return property.Value.ToString().NullIfWhiteSpace();
    }

}