using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public class PropertyConverterCollection : BuilderCollectionBase<IPropertyConverter> {

        public PropertyConverterCollection(Func<IEnumerable<IPropertyConverter>> converters) : base(converters) { }

    }

}