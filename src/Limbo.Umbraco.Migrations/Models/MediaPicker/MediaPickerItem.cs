using System;
using Newtonsoft.Json;

namespace Limbo.Umbraco.Migrations.Models.MediaPicker {

    public class MediaPickerItem {

        [JsonProperty("key")]
        public Guid Key { get; }

        [JsonProperty("mediaKey")]
        public Guid MediaKey { get; }

        public MediaPickerItem(Guid key, Guid mediaKey) {
            Key = key;
            MediaKey = mediaKey;
        }

    }

}