using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public class UmbracoTinyMceConverter : PropertyConverterBase {

        public UmbracoTinyMceConverter(IMigrationsService migrationsService, IMigrationsHttpClient migrationsHttpClient) : base(migrationsService, migrationsHttpClient) { }

        public override bool IsConverter(LegacyProperty property) {
            return property.EditorAlias is "Umbraco.TinyMCEv3";
        }

        public override object? Convert(LegacyEntity owner, LegacyProperty property) {
            string value = property.Value.ToString();
            return string.IsNullOrWhiteSpace(value) ? null : MigrationsService.ConvertRte(property.Value.ToString());
        }

    }

}