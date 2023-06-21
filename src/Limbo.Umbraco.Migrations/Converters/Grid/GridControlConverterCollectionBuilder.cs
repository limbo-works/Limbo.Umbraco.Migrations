using Umbraco.Cms.Core.Composing;

namespace Limbo.Umbraco.Migrations.Converters.Grid {

    public class GridControlConverterCollectionBuilder : LazyCollectionBuilderBase<GridControlConverterCollectionBuilder, GridControlConverterCollection, IGridControlConverter> {

        protected override GridControlConverterCollectionBuilder This => this;

    }

}