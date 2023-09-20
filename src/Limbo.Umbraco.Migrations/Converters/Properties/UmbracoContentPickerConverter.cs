using System.Linq;
using Limbo.Umbraco.Migrations.Constants;
using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Content;
using Limbo.Umbraco.MigrationsClient.Models.Properties;
using Skybrud.Essentials.Strings.Extensions;
using Umbraco.Cms.Core;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public class UmbracoContentPickerConverter : PropertyConverterBase {

        public UmbracoContentPickerConverter(IMigrationsService migrationsService, IMigrationsClient migrationsClient) : base(migrationsService, migrationsClient) { }

        public override bool IsConverter(ILegacyElement owner, ILegacyProperty property) {
            return property.EditorAlias is PropertyEditorAliases.Umbraco.ContentPicker;
        }

        public override object? Convert(ILegacyElement owner, ILegacyProperty property) {

            // Get the first reference ID (not sure if there can be mroe than once)
            int id = property.Value.ToString().ToInt32Array().FirstOrDefault();
            if (id == 0) return null;

            // Get a reference to the legacy content with the referenced ID
            LegacyContent content = MigrationsClient.GetContentById(id);

            // Return a new GUID UDI value wrapping the content item's key
            return new GuidUdi(UmbracoEntityTypes.Content, content.Key).ToString();

        }

    }

}