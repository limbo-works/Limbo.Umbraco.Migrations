namespace Limbo.Umbraco.Migrations.Models.BlockList {

    public class BlockListItem {

        public BlockListContentData Content { get; set; }

        public BlockListSettingsData? Settings { get; set; }

        public BlockListItem(BlockListContentData content) {
            Content = content;
        }

        public BlockListItem(BlockListContentData content, BlockListSettingsData? settings) {
            Content = content;
            Settings = settings;
        }

    }

}