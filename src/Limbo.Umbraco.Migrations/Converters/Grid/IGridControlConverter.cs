using Limbo.Umbraco.Migrations.Models.BlockList;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Skybrud.Grid;

namespace Limbo.Umbraco.Migrations.Converters.Grid;

/// <summary>
/// Interface describing a converter for converting a <see cref="GridControl"/> into a <see cref="BlockListItem"/>.
/// </summary>
public interface IGridControlConverter {

    public bool IsConverter(GridControl control);

    public void Convert(ILegacyElement owner, GridControl control, BlockListModel blockList);

}