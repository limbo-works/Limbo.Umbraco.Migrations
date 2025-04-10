using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Properties;
using Limbo.Umbraco.MigrationsClient.Models.Skybrud.Grid;

namespace Limbo.Umbraco.Migrations.Converters.Models.Skybrud;

public interface IGridDataModelConverter {

    object? Convert(ILegacyElement owner, ILegacyProperty property, GridDataModel gridData);

}