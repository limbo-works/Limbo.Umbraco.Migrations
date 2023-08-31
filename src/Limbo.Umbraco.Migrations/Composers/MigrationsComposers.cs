using Limbo.Umbraco.Migrations.Converters.Grid;
using Limbo.Umbraco.Migrations.Converters.Models.Archetype;
using Limbo.Umbraco.Migrations.Converters.Models.Skybrud;
using Limbo.Umbraco.Migrations.Converters.Properties;
using Limbo.Umbraco.Migrations.Services;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Limbo.Umbraco.Migrations.Composers {

    //// <inheritdoc />
    public class MigrationsComposer : IComposer {

        //// <inheritdoc />
        public void Compose(IUmbracoBuilder builder) {

            // TODO: Should we only set up the package for "Development" ?

            builder
                .WithCollectionBuilder<PropertyConverterCollectionBuilder>()
                .Add(() => builder.TypeLoader.GetTypes<IPropertyConverter>());

            builder
                .WithCollectionBuilder<GridControlConverterCollectionBuilder>()
                .Add(() => builder.TypeLoader.GetTypes<IGridControlConverter>());

            builder.Services.AddSingleton<MigrationsServiceDependencies>();
            builder.Services.AddSingleton<IArchetypeModelConverter, ArchetypeModelConverter>();
            builder.Services.AddSingleton<IGridDataModelConverter, GridDataModelConverter>();

            builder.ManifestFilters().Append<MigrationsManifestFilter>();


        }

    }

}