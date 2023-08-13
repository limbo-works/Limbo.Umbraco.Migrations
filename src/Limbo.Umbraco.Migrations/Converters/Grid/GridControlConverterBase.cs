using Limbo.Umbraco.Migrations.Models.BlockList;
using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Skybrud.Umbraco.GridData.Models;

namespace Limbo.Umbraco.Migrations.Converters.Grid {

    public abstract class GridControlConverterBase : IGridControlConverter {

        public IMigrationsService MigrationsService { get; }

        public IMigrationsClient MigrationsClient { get; }

        protected GridControlConverterBase(IMigrationsService migrationsService, IMigrationsClient migrationsClient) {
            MigrationsService = migrationsService;
            MigrationsClient = migrationsClient;
        }

        public abstract bool IsConverter(GridControl control);

        public abstract void Convert(ILegacyElement owner, GridControl control, BlockListModel blockList);

    }

}