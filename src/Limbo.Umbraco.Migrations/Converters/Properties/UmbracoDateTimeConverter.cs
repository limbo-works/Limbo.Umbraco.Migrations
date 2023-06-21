using System.Globalization;
using System;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public class UmbracoDateTimeConverter : PropertyConverterBase {

        public UmbracoDateTimeConverter(IMigrationsService migrationsService, IMigrationsHttpClient migrationsHttpClient) : base(migrationsService, migrationsHttpClient) { }

        public override bool IsConverter(LegacyProperty property) {
            return property.EditorAlias is "Umbraco.Date" or "Umbraco.DateTime";
        }

        public override object? Convert(LegacyEntity owner, LegacyProperty property) {

            // Get the value as a string
            string value = property.Value.ToString();
            if (string.IsNullOrWhiteSpace(value)) return null;

            // Parse the string into a DateTime instance
            return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);

        }

    }

}