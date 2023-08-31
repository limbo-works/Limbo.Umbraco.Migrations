using System;
using System.Linq;
using Limbo.Umbraco.Migrations.Converters.Grid;
using Limbo.Umbraco.Migrations.Models.BlockList;
using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Properties;
using Skybrud.Umbraco.GridData.Models;

namespace Limbo.Umbraco.Migrations.Converters.Models.Skybrud;

public class GridDataModelConverter : IGridDataModelConverter {

    private readonly GridControlConverterCollection _gridControlConverters;

    #region Properties

    public IMigrationsService MigrationsService { get; }

    #endregion

    #region Constructors

    public GridDataModelConverter(IMigrationsService migrationsService, GridControlConverterCollection gridControlConverters) {
        MigrationsService = migrationsService;
        _gridControlConverters = gridControlConverters;
    }

    #endregion

    #region Member methods

    public virtual object? Convert(ILegacyElement owner, ILegacyProperty property, GridDataModel gridData) {

        // Initialize a new block list
        BlockListModel blockList = new();

        // Iterate through the grid and convert each control
        foreach (GridSection section in gridData.Sections) ConvertGridSection(owner, property, section, blockList);

        // Return the block list value
        return blockList.Items.Count == 0 ? null : blockList;

    }

    protected virtual void ConvertGridSection(ILegacyElement owner, ILegacyProperty property, GridSection section, BlockListModel blockList) {
        foreach (GridRow row in section.Rows) ConvertGridRow(owner, property, row, blockList);
    }

    protected virtual void ConvertGridRow(ILegacyElement owner, ILegacyProperty property, GridRow row, BlockListModel blockList) {
        foreach (GridArea area in row.Areas) ConvertGridArea(owner, property, area, blockList);
    }

    protected virtual void ConvertGridArea(ILegacyElement owner, ILegacyProperty property, GridArea area, BlockListModel blockList) {
        foreach (GridControl control in area.Controls) ConvertGridControl(owner, property, control, blockList);
    }

    protected virtual void ConvertGridControl(ILegacyElement owner, ILegacyProperty property, GridControl control, BlockListModel blockList) {

        // Look for a converter that knows how to convert the grid control
        if (_gridControlConverters.FirstOrDefault(x => x.IsConverter(control)) is { } converter) {
            converter.Convert(owner, control, blockList);
            return;
        }

        throw new Exception($"Unknown grid element: {control.Editor.Alias}\r\n\r\n{control}\r\n\r\n{control.JObject}");

    }

    #endregion

}