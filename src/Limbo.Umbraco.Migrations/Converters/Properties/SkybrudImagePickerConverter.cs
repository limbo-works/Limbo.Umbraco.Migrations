using System;
using Limbo.Umbraco.Migrations.Exceptions;
using Limbo.Umbraco.Migrations.Models.MediaPicker;
using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Properties;
using Limbo.Umbraco.MigrationsClient.Models.Skybrud.ImagePicker;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Newtonsoft;
using Skybrud.Essentials.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Limbo.Umbraco.Migrations.Converters.Properties;

/// <summary>
/// Default converter for migrating values from our <strong>Skybrud.ImagePicker</strong>.
///
/// The Umbraco 7 version of the package saved a JSON model, and support specifying a title for the overall list, as
/// well as a title, description and link for each image item. Since there isn't a similar property for newer versions
/// of Umbraco, we can't really do much with this extra information out of the box. As such, the implementation in this
/// converter will only look at the selected images, and return a new <see cref="MediaPickerList"/> mimicking the model
/// of Umbraco's V3 media picker.
///
/// For the Umbraco 8 version of the package, we choose to drop support for the additinal properties supported in the
/// Umbraco 7 package, and instead left this to be handled by block list style property editors, and as a result of this,
/// only saved a comma separated list of UDIs. The converter will iterate through these, and create a new
/// <see cref="MediaPickerItem"/> instance for each, and then wrap these in a new <see cref="MediaPickerList"/> again
/// mimicking the model of Umbraco's V3 media picker.
/// </summary>
public class SkybrudImagePickerConverter : PropertyConverterBase {

    public SkybrudImagePickerConverter(IMigrationsService migrationsService, IMigrationsClient migrationsClient) : base(migrationsService, migrationsClient) { }

    public override bool IsConverter(ILegacyElement owner, ILegacyProperty property) {
        return property.EditorAlias is "Skybrud.ImagePicker.Image";
    }

    public override object? Convert(ILegacyElement owner, ILegacyProperty property) {

        switch (property.Value.Type) {

            case JTokenType.Null:
                return null;

            // If the token type is an object, we can pass on the value to the "ConvertFromJson" right away
            case JTokenType.Object:
                return ConvertFromJson(owner, property, (JObject) property.Value);

            // If the token type is an object, we need to check the value a bit more, the value could be a serialized
            // JSON object from the Umbraco 7 package, and a comma separated UDI list from the Umbraco 8 package, which
            // we need to handle differently
            case JTokenType.String:
                string str = property.Value.ToString();
                return str.DetectIsJson() ? JsonUtils.ParseJsonObject(str, x => ConvertFromJson(owner, property, x)) : ConvertFromUdis(owner, property, str);

            default:
                throw new MigrationsConvertPropertyException(owner, property, $"Unsupported JSON token type: {property.Value.Type}");

        }

    }

    protected object? ConvertFromJson(ILegacyElement owner, ILegacyProperty property, JObject source) {

        ImagePickerList sourceValue = new(source);

        MediaPickerList list = new();
        int i = 0;

        // Iterate through the image list
        foreach (ImagePickerItem item in sourceValue.Items) {

            // Skip if the image ID is zero
            if (item.ImageId == 0) continue;

            // Generate a unique but reproduceable GUID key for the new item
            Guid key = SecurityUtils.GetMd5Guid($"imagePickerItem:{owner.Key}:{i++}");

            // Get a reference to the media
            IMedia? media = MigrationsService.ImportMedia(item.ImageId);
            if (media is null) continue;

            // Add a new media item
            list.Add(new MediaPickerItem(key, media.Key));

        }

        // Only return the list if we have any items
        return list.Count == 0 ? null : list;

    }

    protected object? ConvertFromUdis(ILegacyElement owner, ILegacyProperty property, string source) {

        MediaPickerList list = new();
        int i = 0;

        foreach (string udi in source.Split(',')) {

            // Skip if not a valid UDI
            if (!MigrationsService.TryParseUdi(udi, out GuidUdi? guidUdi)) continue;

            // Generate a unique but reproduceable GUID key for the new item
            Guid key = SecurityUtils.GetMd5Guid($"imagePickerItem:{owner.Key}:{i++}");

            // Get a reference to the media
            IMedia? media = MigrationsService.ImportMedia(guidUdi.Guid);
            if (media is null) continue;

            // Add a new media item
            list.Add(new MediaPickerItem(key, media.Key));

        }

        // Only return the list if we have any items
        return list.Count == 0 ? null : list;

    }

}