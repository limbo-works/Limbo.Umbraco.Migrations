using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Skybrud.Essentials.Strings.Extensions;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public class SkybrudTextboxConverter : PropertyConverterBase {

        public SkybrudTextboxConverter(IMigrationsService migrationsService, IMigrationsHttpClient migrationsHttpClient) : base(migrationsService, migrationsHttpClient) { }

        public override bool IsConverter(LegacyProperty property) {
            return property.EditorAlias is "Skybrud.CharLimitEditor";
        }

        public override object? Convert(LegacyEntity owner, LegacyProperty property) {
            return property.Value.ToString().NullIfWhiteSpace();
        }

    }

}