using System;
using System.Linq;
using Limbo.Umbraco.Migrations.Constants;
using Limbo.Umbraco.Migrations.Converters.Other.Archetype;
using Limbo.Umbraco.Migrations.Exceptions;
using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Archetype;
using Limbo.Umbraco.MigrationsClient.Models.Properties;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public class ArchetypeConverter : PropertyConverterBase {
        private readonly IServiceProvider _serviceProvider;

        #region Constructors

        public ArchetypeConverter(IMigrationsService migrationsService, IMigrationsClient migrationsHttpClient, IServiceProvider serviceProvider) : base(migrationsService, migrationsHttpClient) {
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Member methods

        public override bool IsConverter(ILegacyProperty property) {
            return property.EditorAlias == PropertyEditorAliases.Archetype;
        }

        public override object? Convert(ILegacyElement owner, ILegacyProperty property) {

            if (property.Value is not JObject json) throw new MigrationsException($"Unexpected property value type: {property.Value.GetType()}") ;

            // Deserialize the JSON into a "ArchetypeModel"
            ArchetypeModel model = ArchetypeModel.Parse(json);
            if (model.Fieldsets.Count == 0) return null;

            // Get the alias of the first fieldset
            string alias = model.Fieldsets[0].Alias;

            // At least for now, we don't really (and probably don't need to) support mixed Archetype fieldsets
            if (model.Fieldsets.Any(x => x.Alias != alias)) throw new MigrationsException("Archetype model has fieldsets of mixed types :(");

            // Get a reference to the Achetype model converter (throw an exception if no implementation has been configured)
            IArchetypeModelConverter converter = _serviceProvider.GetRequiredService<IArchetypeModelConverter>();

            // Convert the Archetype model to something else
            return converter.Convert(model, owner, property);

        }

        #endregion

    }

}