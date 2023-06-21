using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Skybrud.Essentials.Strings.Extensions;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public class UmbracoTextboxConverter : PropertyConverterBase {

        public UmbracoTextboxConverter(IMigrationsService migrationsService, IMigrationsHttpClient migrationsHttpClient) : base(migrationsService, migrationsHttpClient) { }

        public override bool IsConverter(LegacyProperty property) {
            return property.EditorAlias is "Umbraco.Textbox" or "Umbraco.TextboxMultiple" or "Umbraco.TrueFalse";
        }

        public override object? Convert(LegacyEntity owner, LegacyProperty property) {
            return property.Value.ToString().NullIfWhiteSpace();
        }

    }

}