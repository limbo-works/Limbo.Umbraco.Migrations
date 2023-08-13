using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using System;
using Limbo.Umbraco.MigrationsClient.Models.Properties;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public class UmbracoMultiNodeTreePickerConverter : PropertyConverterBase {

        public UmbracoMultiNodeTreePickerConverter(IMigrationsService migrationsService, IMigrationsClient migrationsClient) : base(migrationsService, migrationsClient) { }

        public override bool IsConverter(ILegacyProperty property) {
            return property.EditorAlias is "Umbraco.MultiNodeTreePicker2";
        }

        public override object? Convert(ILegacyElement owner, ILegacyProperty property) {
            string value = property.Value.ToString();
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (!value.StartsWith("umb://")) throw new Exception("WTF?");
            return value;
        }

    }

}