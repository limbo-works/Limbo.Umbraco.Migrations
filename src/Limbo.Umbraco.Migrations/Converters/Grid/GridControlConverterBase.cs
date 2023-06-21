using Limbo.Umbraco.Migrations.Models.BlockList;
using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Skybrud.Umbraco.GridData.Models;

namespace Limbo.Umbraco.Migrations.Converters.Grid {

    public abstract class GridControlConverterBase : IGridControlConverter {

        public IMigrationsService MigrationsService { get; }

        public IMigrationsHttpClient MigrationsHttpClient { get; }

        protected GridControlConverterBase(IMigrationsService migrationsService, IMigrationsHttpClient migrationsHttpClient) {
            MigrationsService = migrationsService;
            MigrationsHttpClient = migrationsHttpClient;
        }

        public abstract bool IsConverter(GridControl control);

        public abstract void Convert(LegacyEntity owner, GridControl control, BlockListModel blockList);

    }

}