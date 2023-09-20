using System;
using Limbo.Umbraco.Migrations.Converters.Models.Skybrud;
using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Properties;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Newtonsoft;
using Skybrud.Umbraco.GridData.Factories;
using Skybrud.Umbraco.GridData.Models;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public class UmbracoGridPropertyConverter : PropertyConverterBase {

        private readonly IGridFactory _gridFactory;
        private readonly IGridDataModelConverter _gridDataModelConverter;

        public UmbracoGridPropertyConverter(IMigrationsService migrationsService, IMigrationsClient migrationsClient, IGridFactory gridFactory, IGridDataModelConverter gridDataModelConverter) : base(migrationsService, migrationsClient) {
            _gridFactory = gridFactory;
            _gridDataModelConverter = gridDataModelConverter;
        }

        public override bool IsConverter(ILegacyElement owner, ILegacyProperty property) {
            return property.EditorAlias is "Umbraco.Grid";
        }

        public override object? Convert(ILegacyElement owner, ILegacyProperty property) {

            if (!JsonUtils.TryParseJsonObject(property.Value.ToString(), out JObject? json)) return null;

            try {

                // Parse the legacy JSON into a GridDataModel instance (control values won't be strongly typed)
                GridDataModel model = _gridFactory.CreateGridModel(null!, null!, json, false);

                // Convert the grid data model to something else
                return _gridDataModelConverter.Convert(owner, property, model);

            } catch (Exception ex) {

                throw new Exception($"Converting grid value failed for entity '{owner.Name}'\r\n\r\nID: {owner.Key}\r\nKey: {owner.Key}\r\n\r\n\r\n", ex);

            }

        }

    }

}