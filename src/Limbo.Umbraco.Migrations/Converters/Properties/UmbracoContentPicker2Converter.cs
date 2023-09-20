using System;
using Limbo.Umbraco.Migrations.Constants;
using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Properties;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public class UmbracoContentPicker2Converter : PropertyConverterBase {

        public UmbracoContentPicker2Converter(IMigrationsService migrationsService, IMigrationsClient migrationsClient) : base(migrationsService, migrationsClient) { }

        public override bool IsConverter(ILegacyElement owner, ILegacyProperty property) {
            return property.EditorAlias is PropertyEditorAliases.Umbraco.ContentPicker2;
        }

        public override object? Convert(ILegacyElement owner, ILegacyProperty property) {

            // Get the value as a string
            string strValue = property.Value.ToString();

            // Return null if the value is null or white space
            if (string.IsNullOrWhiteSpace(strValue)) return null;

            // If we have a value at this point, it should be a UDI
            if (!strValue.StartsWith("umb://")) throw new Exception("WTF?");

            // As UDIs use the GUID key opposed to the numeric ID, we can reutnr the value without any changes
            return strValue;

        }

    }

}