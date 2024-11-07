namespace Limbo.Umbraco.Migrations.Models.BlockList;

public static class BlockListExtensions {

    public static TContent AppendAsItem<TContent>(this TContent content, BlockListModel blockList) where TContent : BlockListContentData {
        blockList.AddItem(content);
        return content;
    }

    public static TItem AppendTo<TItem>(this TItem item, BlockListModel model) where TItem : BlockListItem {
        model.AddItem(item);
        return item;
    }

}