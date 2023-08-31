using Limbo.Umbraco.MigrationsClient.Models;
using Skybrud.Umbraco.GridData.Models;
using Limbo.Umbraco.MigrationsClient.Models.Properties;

namespace Limbo.Umbraco.Migrations.Converters.Models.Skybrud;

public interface IGridDataModelConverter {

    object? Convert(ILegacyElement owner, ILegacyProperty property, GridDataModel gridData);

}