using Umbraco.Cms.Core.Composing;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public class PropertyConverterCollectionBuilder : LazyCollectionBuilderBase<PropertyConverterCollectionBuilder, PropertyConverterCollection, IPropertyConverter> {

        protected override PropertyConverterCollectionBuilder This => this;

    }

}