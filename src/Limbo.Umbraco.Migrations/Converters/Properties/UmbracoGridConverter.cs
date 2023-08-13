using System;
using System.Linq;
using Limbo.Umbraco.Migrations.Converters.Grid;
using Limbo.Umbraco.Migrations.Models.BlockList;
using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Properties;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Newtonsoft;
using Skybrud.Umbraco.GridData.Factories;
using Skybrud.Umbraco.GridData.Models;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public class UmbracoGridConverter : PropertyConverterBase {

        private readonly IGridFactory _gridFactory;
        private readonly GridControlConverterCollection _gridControlConverters;

        public UmbracoGridConverter(IMigrationsService migrationsService, IMigrationsClient migrationsClient, IGridFactory gridFactory, GridControlConverterCollection gridControlConverters) : base(migrationsService, migrationsClient) {
            _gridFactory = gridFactory;
            _gridControlConverters = gridControlConverters;
        }

        public override bool IsConverter(ILegacyProperty property) {
            return property.EditorAlias is "Umbraco.Grid";
        }

        public override object? Convert(ILegacyElement owner, ILegacyProperty property) {

            if (!JsonUtils.TryParseJsonObject(property.Value.ToString(), out JObject? json)) return null;

            try {

                // Parse the legacy JSON into a GridDataModel instance (control values won't be strongly typed)
                GridDataModel grid = _gridFactory.CreateGridModel(null!, null!, json, false);

                // Initialize a new block list
                BlockListModel blockList = new();

                // Iterate through the grid and convert each control
                foreach (GridSection section in grid.Sections) {
                    foreach (GridRow row in section.Rows) {
                        foreach (GridArea area in row.Areas) {
                            foreach (GridControl control in area.Controls) {
                                ConvertGridControl(owner, control, blockList);
                            }
                        }
                    }
                }

                // Return the block list value
                return blockList.Items.Count == 0 ? null : blockList;

            } catch (Exception ex) {

                throw new Exception($"Converting grid value failed for entity '{owner.Name}'\r\n\r\nID: {owner.Id}\r\nKey: {owner.Key}\r\n\r\n\r\n", ex);

            }

        }

        protected virtual void ConvertGridControl(ILegacyElement owner, GridControl control, BlockListModel blockList) {

            // Look for a converter that knows how to convert the grid control
            if (_gridControlConverters.FirstOrDefault(x => x.IsConverter(control)) is { } converter) {
                converter.Convert(owner, control, blockList);
                return;
            }

            throw new Exception($"Unknown grid element: {control.Editor.Alias}\r\n\r\n{control}\r\n\r\n{control.JObject}");

        }

    }

}