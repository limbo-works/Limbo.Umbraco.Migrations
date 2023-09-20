using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using System.Linq;
using Limbo.Umbraco.MigrationsClient.Models.Properties;
using Skybrud.Essentials.Strings.Extensions;
using Umbraco.Cms.Core;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public class UmbracoMultiNodeTreePickerConverter : PropertyConverterBase {

        public UmbracoMultiNodeTreePickerConverter(IMigrationsService migrationsService, IMigrationsClient migrationsClient) : base(migrationsService, migrationsClient) { }

        public override bool IsConverter(ILegacyElement owner, ILegacyProperty property) {
            return property.EditorAlias is "Umbraco.MultiNodeTreePicker" or "Umbraco.MultiNodeTreePicker2";
        }

        public override object? Convert(ILegacyElement owner, ILegacyProperty property) {

            string value = property.Value.ToString();

            // If the value is empty or white space, we can return null right away as no content has been picked
            if (string.IsNullOrWhiteSpace(value)) return null;

            // If the value starts with "umb://" we know that selected items are stored as UDIs, and we can return the value without any modifications
            if (value.StartsWith("umb://")) return value;

            // The legacy MNTP property editor saved a comma separated list of numeric IDs, so we try to parse that
            int[] ids = value.ToInt32Array();
            if (ids.Length == 0) return null;

            // If we found any numeric IDs, we look up the legacy content to find it's GUID key,
            // and then store it's UDI value instead. We don't import the content just yet, so we
            // as property may belong to an ancestor or the selected content node, and trying to
            // import the content code could then lead to recursive loops. Content items are
            // imported re-using their original GUID keys, so the references to the selected
            // content items should still work once they are imported at a later point in the
            // import process
            return ids
                .Select(id => MigrationsClient.GetContentById(id))
                .Select(content => new GuidUdi(global::Umbraco.Cms.Core.Constants.UdiEntityType.Document, content.Key))
                .Join(",");

        }

    }

}