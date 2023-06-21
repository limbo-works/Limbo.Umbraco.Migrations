using Limbo.Umbraco.Migrations.Models.BlockList;
using Limbo.Umbraco.MigrationsClient.Models;
using Skybrud.Umbraco.GridData.Models;

namespace Limbo.Umbraco.Migrations.Converters.Grid {

    /// <summary>
    /// Interface describing a converter for converting a <see cref="GridControl"/> into a <see cref="BlockListItem"/>.
    /// </summary>
    public interface IGridControlConverter {

        public bool IsConverter(GridControl control);

        public void Convert(LegacyEntity owner, GridControl control, BlockListModel blockList);

    }

}