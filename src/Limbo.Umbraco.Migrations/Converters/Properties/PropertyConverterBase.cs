using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Properties;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public abstract class PropertyConverterBase : IPropertyConverter {

        public IMigrationsService MigrationsService { get; }

        public IMigrationsClient MigrationsClient { get; }

        protected PropertyConverterBase(IMigrationsService migrationsService, IMigrationsClient migrationsClient) {
            MigrationsService = migrationsService;
            MigrationsClient = migrationsClient;
        }

        public abstract bool IsConverter(ILegacyProperty property);

        public abstract object? Convert(ILegacyElement owner, ILegacyProperty property);

    }

}