using System;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Archetype;
using Limbo.Umbraco.MigrationsClient.Models.Properties;

namespace Limbo.Umbraco.Migrations.Converters.Models.Archetype;

public class ArchetypeModelConverter : IArchetypeModelConverter {

    public object Convert(ILegacyElement? owner, ILegacyProperty? property, ArchetypeModel model) {
        throw new NotImplementedException();
    }

}