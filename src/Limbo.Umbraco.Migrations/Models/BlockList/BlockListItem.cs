using System;
using System.Linq.Expressions;
using Skybrud.Essentials.Reflection;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Limbo.Umbraco.Migrations.Models.BlockList;

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

public class BlockListItem<TContent, TSettings> : BlockListItem where TContent : PublishedElementModel where TSettings : PublishedElementModel {

    public new BlockListContentData<TContent> Content => (BlockListContentData<TContent>) base.Content;

    public new BlockListSettingsData<TSettings> Settings => (BlockListSettingsData<TSettings>) base.Settings!;

    public BlockListItem(BlockListContentData<TContent> content, BlockListSettingsData<TSettings> settings) : base(content, settings) { }

    public BlockListItem<TContent, TSettings> SetContentValue<TProperty>(Expression<Func<TContent, TProperty>> selector, object? value) {

        // Get the name/alias of the property
        string alias = ReflectionUtils.GetPropertyInfo(selector).Name;

        // Not sure how much casing matters, so we better lookup the correct casing of the property type
        IPublishedPropertyType? propertyType = Content.ContentType.GetPropertyType(alias);
        if (propertyType is null) throw new Exception($"Property type with alias '{alias}' not found for content type '{Content.ContentType.Alias}'.");

        // Set the property value
        Content.SetValue(propertyType.Alias, value);

        return this;

    }

    public BlockListItem<TContent, TSettings> SetSettingsValue<TProperty>(Expression<Func<TSettings, TProperty>> selector, object? value) {

        // Get the name/alias of the property
        string alias = ReflectionUtils.GetPropertyInfo(selector).Name;

        // Not sure how much casing matters, so we better lookup the correct casing of the property type
        IPublishedPropertyType? propertyType = Settings.ContentType.GetPropertyType(alias);
        if (propertyType is null) throw new Exception($"Property type with alias '{alias}' not found for content type '{Settings.ContentType.Alias}'.");

        // Set the property value
        Settings.SetValue(propertyType.Alias, value);

        return this;

    }

}