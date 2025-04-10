using Limbo.Umbraco.Migrations.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Newtonsoft;
using StackExchange.Profiling.Internal;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Limbo.Umbraco.Migrations.Json.Newtonsoft;

public class MigrationsJsonUtils {

    public static JToken FromObject(object value) {
        return StaticServiceProvider.Instance.GetRequiredService<IMigrationsService>().ToJson(value);
    }

    public static string Serialize(object value) {
        return FromObject(value).ToString(Formatting.Indented);
    }

    public static void Expand(JObject json, bool recursive = true) {

        foreach (JProperty property in json.Properties()) {

            if (property.Value.Type == JTokenType.String) {
                string value = (string) property.Value!;
                if (value.DetectIsJson() && JsonUtils.TryParseJsonToken(value, out JToken? valueJson)) {
                    property.Value = valueJson;
                }
            } else if (property.Value is JObject obj && recursive) {
                Expand(obj, recursive);
            } else if (property.Value is JArray array && recursive) {
                Expand(array, recursive);
            }

        }

    }

    public static void Expand(JToken json, bool recursive = true) {

        switch (json) {

            case JObject obj:
                foreach (JProperty property in obj.Properties()) {
                    if (property.Value.Type == JTokenType.String) {
                        string value = (string) property.Value!;
                        if (value.DetectIsJson() && JsonUtils.TryParseJsonToken(value, out JToken? valueJson)) {
                            property.Value = valueJson;
                            Expand(valueJson, recursive);
                        }
                    } else {
                        Expand(property.Value, recursive);
                    }
                }
                break;

            case JArray array:
                foreach (JToken item in array) {
                    Expand(item, recursive);
                }
                break;

        }

    }

    public static JObject Copy(JObject json) {
        return JsonUtils.ParseJsonObject(json.ToString());
    }

}