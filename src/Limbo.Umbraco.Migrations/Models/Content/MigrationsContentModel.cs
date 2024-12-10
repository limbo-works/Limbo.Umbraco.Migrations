using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Limbo.Umbraco.MigrationsClient.Models.Content;
using Microsoft.Extensions.DependencyInjection;
using Skybrud.Essentials.Reflection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Limbo.Umbraco.Migrations.Models.Content;

public class MigrationsContentModel {

    public LegacyContent Entity { get; }

    public Guid? ParentKey { get; set; }

    public Guid Key { get; set; }

    public string Name { get; set; }

    public string ContentTypeAlias { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime UpdateDate { get; set; }

    public Dictionary<string, object?> Properties { get; } = [];

    public MigrationsContentModel(LegacyContent entity) {
        Entity = entity;
        ParentKey = entity.Path.LastOrDefault()?.Key;
        Key = entity.Key;
        Name = entity.Name;
        ContentTypeAlias = entity.ContentTypeAlias;
        CreateDate = entity.CreateDate.DateTime;
        UpdateDate = entity.UpdateDate.DateTime;
    }

    public static MigrationsContentModel<TModel> Create<TModel>(LegacyContent entity) where TModel : PublishedContentModel {
        return new MigrationsContentModel<TModel>(entity, GetModelType<TModel>());
    }

    private static IPublishedContentType GetModelType<T>() where T : PublishedContentModel {
        var t = typeof(T);
        var field = t.GetField("ModelTypeAlias");
        string alias = (string) field!.GetValue(null)!;
        return StaticServiceProvider.Instance.GetRequiredService<IUmbracoContextAccessor>().GetRequiredUmbracoContext().Content!.GetContentType(alias)!;
    }

}

public class MigrationsContentModel<TModel> : MigrationsContentModel where TModel : PublishedContentModel {

    public IPublishedContentType ContentType { get; }

    public MigrationsContentModel(LegacyContent entity, IPublishedContentType contentType) : base(entity) {
        Key = entity.Key;
        ContentTypeAlias = contentType.Alias;
        ContentType = contentType;
    }

    public MigrationsContentModel<TModel> SetValue<TProperty>(Expression<Func<TModel, TProperty>> selector, object? value) {

        // Get the name/alias of the property
        string alias = ReflectionUtils.GetPropertyInfo(selector).Name;

        // Not sure how much casing matters, so we better lookup the correct casing of the property type
        IPublishedPropertyType? propertyType = ContentType.GetPropertyType(alias);
        if (propertyType is null) throw new Exception($"Property type with alias '{alias}' not found for content type '{ContentType.Alias}'.");

        // Set the property value
        if (value is null) {
            Properties.Remove(alias);
        } else {
            Properties[propertyType.Alias] = value;
        }

        return this;

    }

}