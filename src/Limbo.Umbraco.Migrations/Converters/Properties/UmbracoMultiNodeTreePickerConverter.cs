using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using System;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public class UmbracoMultiNodeTreePickerConverter : PropertyConverterBase {

        public UmbracoMultiNodeTreePickerConverter(IMigrationsService migrationsService, IMigrationsHttpClient migrationsHttpClient) : base(migrationsService, migrationsHttpClient) { }

        public override bool IsConverter(LegacyProperty property) {
            return property.EditorAlias is "Umbraco.MultiNodeTreePicker2";
        }

        public override object? Convert(LegacyEntity owner, LegacyProperty property) {
            string value = property.Value.ToString();
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (!value.StartsWith("umb://")) throw new Exception("WTF?");
            return value;
        }

    }

}