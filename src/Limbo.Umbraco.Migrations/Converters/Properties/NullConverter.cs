using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Properties;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public class NullConverter : PropertyConverterBase {

        public NullConverter(IMigrationsService migrationsService, IMigrationsClient migrationsClient) : base(migrationsService, migrationsClient) { }

        public override bool IsConverter(ILegacyProperty property) {
            return property.EditorAlias switch {
                "CodeMonkey.Seperator" => true,
                "Skybrud.Umbraco.Redirects" => true,
                "RankOneResultPreview" => true,
                "RankOneDashboard" => true,
                _ => false
            };
        }

        public override object? Convert(ILegacyElement owner, ILegacyProperty property) {
            return null;
        }

    }

}