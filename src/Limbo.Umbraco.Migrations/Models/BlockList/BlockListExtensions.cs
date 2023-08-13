namespace Limbo.Umbraco.Migrations.Models.BlockList {

    public static class BlockListExtensions {

        public static TContent AppendAsItem<TContent>(this TContent content, BlockListModel blockList) where TContent : BlockListContentData {
            blockList.AddItem(content);
            return content;
        }

    }

}
