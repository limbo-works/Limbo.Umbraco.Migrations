using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Properties;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public interface IPropertyConverter {

        /// <summary>
        /// Returns whether this converter is a converter for the specified <paramref name="property"/>.
        /// </summary>
        /// <param name="owner">The element holding the property.</param>
        /// <param name="property">The property.</param>
        /// <returns><see langword="true"/> if a converter for <paramref name="property"/>; otherwise, <see langword="false"/>.</returns>
        bool IsConverter(ILegacyElement owner, ILegacyProperty property);

        /// <summary>
        /// Returns the converted value from the specified <paramref name="property"/>.
        /// </summary>
        /// <param name="owner">The element holding the property.</param>
        /// <param name="property">The property.</param>
        /// <returns>Tthe converted property value.</returns>
        object? Convert(ILegacyElement owner, ILegacyProperty property);

    }

}