using System;
using Limbo.Umbraco.Migrations.Models.UrlPicker;
using Limbo.Umbraco.Migrations.Services;
using Limbo.Umbraco.MigrationsClient;
using Limbo.Umbraco.MigrationsClient.Models;
using Limbo.Umbraco.MigrationsClient.Models.Properties;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Newtonsoft.Extensions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Limbo.Umbraco.Migrations.Converters.Properties;

public class UmbracoMultiUrlPickerConverter : PropertyConverterBase {

    public UmbracoMultiUrlPickerConverter(IMigrationsService migrationsService, IMigrationsClient migrationsClient) : base(migrationsService, migrationsClient) { }

    public override bool IsConverter(ILegacyElement owner, ILegacyProperty property) {
        return property.EditorAlias is "Umbraco.MultiUrlPicker";
    }

    public override object? Convert(ILegacyElement owner, ILegacyProperty property) {

        if (property.Value is not JArray array) return null;

        UrlPickerList list = new();

        foreach (JToken item in array) {

            if (item is not JObject obj) continue;

            string? name = obj.GetString("name");
            string? target = obj.GetString("target");
            string? udi = obj.GetString("udi");
            string? url = obj.GetString("url");

            if (name is null) throw new Exception($"Name is null for link:\r\n\r\n{obj}");

            LinkType type;
            if (MigrationsService.TryParseUdi(udi, out GuidUdi? guidUdi)) {
                type = guidUdi.EntityType switch {
                    "document" => LinkType.Content,
                    "media" => LinkType.Media,
                    _ => throw new Exception($"Unknown UDI entity type '{guidUdi.EntityType}' for link:\r\n\r\n{obj}")
                };

            } else {
                type = LinkType.External;
            }

            UrlPickerItem urlItem = new(type, name, guidUdi, url, target);

            list.Add(urlItem);

        }

        return list.Count == 0 ? null : list;

    }

}