using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Archetype;
using Limbo.Umbraco.MigrationsClient.Models.Properties;

namespace Limbo.Umbraco.Migrations.Converters.Models.Archetype {

    /// <summary>
    /// Interface describing a converter for converting a <see cref="ArchetypeModel"/> instance into another value.
    /// </summary>
    public interface IArchetypeModelConverter {

        /// <summary>
        /// Converts the specified <paramref name="model"/>.
        /// </summary>
        /// <param name="owner">A reference to the parent element, if any.</param>
        /// <param name="property">A reference to the parent property, if any.</param>
        /// <param name="model">The Achetype model.</param>
        /// <returns></returns>
        object Convert(ILegacyElement owner, ILegacyProperty property, ArchetypeModel model);

    }

}