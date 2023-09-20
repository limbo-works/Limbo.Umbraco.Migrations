using System.Globalization;
using System;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models.Properties;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public class UmbracoDateTimeConverter : PropertyConverterBase {

        public UmbracoDateTimeConverter(IMigrationsService migrationsService, IMigrationsClient migrationsClient) : base(migrationsService, migrationsClient) { }

        public override bool IsConverter(ILegacyElement owner, ILegacyProperty property) {
            return property.EditorAlias is "Umbraco.Date" or "Umbraco.DateTime";
        }

        public override object? Convert(ILegacyElement owner, ILegacyProperty property) {

            // Get the value as a string
            string value = property.Value.ToString();
            if (string.IsNullOrWhiteSpace(value)) return null;

            // Parse the string into a DateTime instance
            return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);

        }

    }

}