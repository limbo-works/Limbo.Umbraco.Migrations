using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;

namespace Limbo.Umbraco.Migrations.Converters.Grid {

    public class GridControlConverterCollection : BuilderCollectionBase<IGridControlConverter> {

        public GridControlConverterCollection(Func<IEnumerable<IGridControlConverter>> converters) : base(converters) { }

    }

}