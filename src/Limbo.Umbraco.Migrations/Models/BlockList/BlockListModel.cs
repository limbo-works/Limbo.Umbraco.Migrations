using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Limbo.Umbraco.Migrations.Models.BlockList {

    [JsonConverter(typeof(BlockListModelJsonConverter))]
    public class BlockListModel {

        public int Count => Items.Count;

        public List<BlockListItem> Items { get; } = new();

        public BlockListModel AddItem(BlockListItem item) {
            Items.Add(item);
            return this;
        }

        public BlockListModel AddItem(BlockListContentData content) {
            Items.Add(new BlockListItem(content));
            return this;
        }

        public BlockListModel AddItem(BlockListContentData content, BlockListSettingsData? settings) {
            Items.Add(new BlockListItem(content, settings));
            return this;
        }

        public string ToJsonString() {
            return JObject.FromObject(this).ToString(Formatting.None);
        }

    }

}