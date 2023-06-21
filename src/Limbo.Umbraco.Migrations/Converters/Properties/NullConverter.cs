using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public class NullConverter : PropertyConverterBase {

        public NullConverter(IMigrationsService migrationsService, IMigrationsHttpClient migrationsHttpClient) : base(migrationsService, migrationsHttpClient) { }

        public override bool IsConverter(LegacyProperty property) {
            return property.EditorAlias switch {
                "CodeMonkey.Seperator" => true,
                "Skybrud.Umbraco.Redirects" => true,
                "RankOneResultPreview" => true,
                "RankOneDashboard" => true,
                _ => false
            };
        }

        public override object? Convert(LegacyEntity owner, LegacyProperty property) {
            return null;
        }

    }

}