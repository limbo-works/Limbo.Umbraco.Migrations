using System;
using Limbo.Umbraco.Migrations.Models.MediaPicker;
using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Security;
using Skybrud.Essentials.Strings.Extensions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Limbo.Umbraco.Migrations.Converters.Properties {

    public class UmbracoMediaPickerConverter : PropertyConverterBase {

        public UmbracoMediaPickerConverter(IMigrationsService migrationsService, IMigrationsClient migrationsClient) : base(migrationsService, migrationsClient) { }

        public override bool IsConverter(ILegacyProperty property) {
            return property.EditorAlias is "Umbraco.MediaPicker2";
        }

        public override object? Convert(ILegacyElement owner, ILegacyProperty property) {

            // Get the value as a string
            string value = property.Value.ToString();

            MediaPickerList list = new();

            int i = 0;

            foreach (string item in value.ToStringArray()) {

                // Generate a unique but reproduceable GUID key for the new item
                Guid key = SecurityUtils.GetMd5Guid($"imagePickerItem:{owner.Key}:{i++}");

                if (!UdiParser.TryParse(item, out Udi? udi)) throw new Exception($"Item is not a valid UDI: {item}");
                if (udi is not GuidUdi guidUdi) throw new Exception($"Item is not a valid GUID UDI: {item}");

                // Get a reference to the media
                IMedia? media = MigrationsService.ImportMedia(guidUdi.Guid);
                if (media is null) continue;

                // Add a new media item
                list.Add(new MediaPickerItem(key, media.Key));

            }

            return list.Count == 0 ? null : JToken.FromObject(list).ToString(Formatting.None);

        }

    }

}