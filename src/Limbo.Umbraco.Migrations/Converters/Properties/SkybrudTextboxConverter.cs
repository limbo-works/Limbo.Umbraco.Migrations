using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Properties;
using Skybrud.Essentials.Strings.Extensions;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public class SkybrudTextboxConverter : PropertyConverterBase {

        public SkybrudTextboxConverter(IMigrationsService migrationsService, IMigrationsClient migrationsClient) : base(migrationsService, migrationsClient) { }

        public override bool IsConverter(ILegacyProperty property) {
            return property.EditorAlias is "Skybrud.CharLimitEditor";
        }

        public override object? Convert(ILegacyElement owner, ILegacyProperty property) {
            return property.Value.ToString().NullIfWhiteSpace();
        }

    }

}