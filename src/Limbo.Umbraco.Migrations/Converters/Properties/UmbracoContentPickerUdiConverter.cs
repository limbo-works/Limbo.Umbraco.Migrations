using System;
using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Properties;

namespace Limbo.Umbraco.Migrations.Converters.Properties;

/// <summary>
/// Umbraco 7 had two different content pickers, where the oldest (<c>Umbraco.ContentPickerAlias</c>) saved a list of
/// numeric IDs, and the newest (<c>Umbraco.ContentPicker2</c>) instead saved a list of UDIs. This converter handles the
/// latter, and will return the saved value "as-is" since the content picker (or MNTP) in newer versions of Umbraco will
/// use the same format.
///
/// Umbraco 8 has a similar UDI based content picker, but uses the alias <c>Umbraco.ContentPicker</c> instead. The
/// converter will also handle these values.
/// </summary>
public class UmbracoContentPickerUdiConverter : UmbracoContentPicker2Converter {

    public UmbracoContentPickerUdiConverter(IMigrationsService migrationsService, IMigrationsClient migrationsClient) : base(migrationsService, migrationsClient) { }

    public override bool IsConverter(ILegacyElement owner, ILegacyProperty property) {
        return property.EditorAlias is "Umbraco.ContentPicker" or "Umbraco.ContentPicker2";
    }

    public override object? Convert(ILegacyElement owner, ILegacyProperty property) {

        // Get the value as a string
        string strValue = property.Value.ToString();

        // Return null if the value is null or white space
        if (string.IsNullOrWhiteSpace(strValue)) return null;

        // If we have a value at this point, it should be a UDI
        if (!strValue.StartsWith("umb://")) throw new Exception("WTF?");

        // As UDIs use the GUID key opposed to the numeric ID, we can return the value without any changes
        return strValue;

    }

}