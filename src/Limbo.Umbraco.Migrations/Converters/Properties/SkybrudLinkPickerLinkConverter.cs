using System;
using Limbo.Umbraco.Migrations.Models.UrlPicker;
using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Properties;
using Limbo.Umbraco.MigrationsClient.Models.Skybrud.LinkPicker;
using Newtonsoft.Json.Linq;

namespace Limbo.Umbraco.Migrations.Converters.Properties;

/// <summary>
/// Converts a single <see cref="LinkPickerItem"/> to a new <see cref="UrlPickerList"/>.
/// </summary>
public class SkybrudLinkPickerLinkConverter : PropertyConverterBase {

    public SkybrudLinkPickerLinkConverter(IMigrationsService migrationsService, IMigrationsClient migrationsClient) : base(migrationsService, migrationsClient) { }

    public override bool IsConverter(ILegacyElement owner, ILegacyProperty property) {
        return property.EditorAlias is "Skybrud.LinkPicker.Link";
    }

    public override object? Convert(ILegacyElement owner, ILegacyProperty property) {

        if (property.Value.Type == JTokenType.Null) return null;

        if (property.Value is not JObject source) throw new Exception("Property value is not an instance of JObject.");

        LinkPickerItem item = LinkPickerItem.Parse(source);

        return MigrationsService.ConvertLinkPickerItemAsList(item);

    }

}