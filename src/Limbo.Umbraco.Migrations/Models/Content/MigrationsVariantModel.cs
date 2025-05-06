using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using Limbo.Umbraco.MigrationsClient.Models.Content;
using Microsoft.Extensions.DependencyInjection;
using Skybrud.Essentials.Reflection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Limbo.Umbraco.Migrations.Models.Content;

public class MigrationsVariantModel {

    public string CultureName { get; }

    public string? Name { get; set; }

    public Dictionary<string, object?> Properties { get; } = [];

    public MigrationsVariantModel(string cultureName) {
        CultureName = cultureName;
    }

    public static MigrationsVariantModel<TModel> Create<TModel>(LegacyContent entity, string cultureName) where TModel : PublishedContentModel {
        return new MigrationsVariantModel<TModel>(cultureName, GetModelType<TModel>());
    }

    public static MigrationsVariantModel<TModel> Create<TModel>(LegacyContent entity, CultureInfo culture) where TModel : PublishedContentModel {
        return new MigrationsVariantModel<TModel>(culture.ToString(), GetModelType<TModel>());
    }

    private static IPublishedContentType GetModelType<T>() where T : PublishedContentModel {
        var t = typeof(T);
        var field = t.GetField("ModelTypeAlias");
        string alias = (string) field!.GetValue(null)!;
        return StaticServiceProvider.Instance.GetRequiredService<IUmbracoContextAccessor>().GetRequiredUmbracoContext().Content!.GetContentType(alias)!;
    }

}

public class MigrationsVariantModel<TModel> : MigrationsVariantModel where TModel : PublishedContentModel {

    public IPublishedContentType ContentType { get; }

    public MigrationsVariantModel(string cultureName, IPublishedContentType contentType) : base(cultureName) {
        ContentType = contentType;
    }

    public MigrationsVariantModel<TModel> SetValue<TProperty>(Expression<Func<TModel, TProperty>> selector, object? value) {

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