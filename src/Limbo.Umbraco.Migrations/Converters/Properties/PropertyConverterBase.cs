using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public abstract class PropertyConverterBase : IPropertyConverter {

        public IMigrationsService MigrationsService { get; }

        public IMigrationsHttpClient MigrationsHttpClient { get; }

        protected PropertyConverterBase(IMigrationsService migrationsService, IMigrationsHttpClient migrationsHttpClient) {
            MigrationsService = migrationsService;
            MigrationsHttpClient = migrationsHttpClient;
        }

        public abstract bool IsConverter(LegacyProperty property);

        public abstract object? Convert(LegacyEntity owner, LegacyProperty property);

    }

}