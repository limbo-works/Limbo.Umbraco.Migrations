﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Limbo.Umbraco.Migrations.Models.BlockList {

    public class BlockListModelJsonConverter : JsonConverter {

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {

            if (value is not BlockListModel model) {
                writer.WriteNull();
                return;
            }

            JArray layout = new();
            JArray contentData = new();
            JArray settingsData = new();

            foreach (BlockListItem item in model.Items) {

                JObject layoutItem = new() {
                    {"contentUdi", item.Content.Udi.ToString()}
                };

                if (item.Settings is not null) layoutItem.Add("settingsUdi", item.Settings.Udi.ToString());

                layout.Add(layoutItem);

                contentData.Add(ConvertData(item.Content));
                if (item.Settings is not null) settingsData.Add(ConvertData(item.Settings));

            }

            JObject blockListValue = new() {
                { "layout", new JObject { { "Umbraco.BlockList", layout } } },
                { "contentData", contentData },
                { "settingsData", settingsData }
            };

            blockListValue.WriteTo(writer);

        }

        private JObject ConvertData(BlockListContentData data) {

            JObject json = new() {
                {"contentTypeKey", data.ContentType.Key},
                {"udi", data.Udi.ToString()}
            };

            foreach (KeyValuePair<string, object?> property in data.Properties) {
                json.Add(property.Key, property.Value is null ? null : JToken.FromObject(property.Value));
            }

            return json;

        }

        private JObject ConvertData(BlockListSettingsData data) {

            JObject json = new() {
                {"contentTypeKey", data.ContentType.Key},
                {"udi", data.Udi.ToString()}
            };

            foreach (KeyValuePair<string, object?> property in data.Properties) {
                json.Add(property.Key, property.Value is null ? null : JToken.FromObject(property.Value));
            }

            return json;

        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType) {
            return false;
        }

    }

}