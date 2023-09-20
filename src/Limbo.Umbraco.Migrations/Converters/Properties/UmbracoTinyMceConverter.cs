using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Properties;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public class UmbracoTinyMceConverter : PropertyConverterBase {

        public UmbracoTinyMceConverter(IMigrationsService migrationsService, IMigrationsClient migrationsClient) : base(migrationsService, migrationsClient) { }

        public override bool IsConverter(ILegacyElement owner, ILegacyProperty property) {
            return property.EditorAlias is "Umbraco.TinyMCEv3";
        }

        public override object? Convert(ILegacyElement owner, ILegacyProperty property) {
            string value = property.Value.ToString();
            return string.IsNullOrWhiteSpace(value) ? null : MigrationsService.ConvertRte(property.Value.ToString());
        }

    }

}