using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Skybrud.Essentials.Reflection;
using Skybrud.Essentials.Security;
using Skybrud.Umbraco.GridData.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Limbo.Umbraco.Migrations.Models.BlockList {

    public class BlockListContentData {

        public IPublishedContentType ContentType { get; }

        public GuidUdi Udi { get; }

        public Dictionary<string, object?> Properties { get; set; } = new();

        public BlockListContentData(Guid key, IPublishedContentType contentType) {
            Udi = new GuidUdi("element", key);
            ContentType = contentType;
        }

        public BlockListContentData(GridControl control, IPublishedContentType contentType) {

            // We need to ensure that each content item has a unique key. Generally we should be able to use the same
            // key as the grid row, but even though this shouldn't be allowed in the legacy site, so rows have more
            // than one control, in which case we creatively need to generate a unique key for those additional
            // controls. Notice that is's important that the calculated key is the same if we repeat it again and again
            // again
            int index1 = control.Row.Areas.IndexOf(control.Area);
            int index2 = control.Area.Controls.IndexOf(control);
            Guid key = SecurityUtils.GetMd5Guid($"{control.Row.Id}#content#{index1}#{index2}");

            // Create an UDI based on the element type and the GUID key
            Udi = new GuidUdi("element", key);

            // Set the content type
            ContentType = contentType;

        }

        public BlockListContentData SetValue(string name, object? value) {
            if (value is null) return this;
            if (value is string str && string.IsNullOrWhiteSpace(str)) return this;
            Properties[name] = value;
            return this;
        }

    }

    public class BlockListContentData<TModel> : BlockListContentData where TModel : PublishedElementModel {

        public BlockListContentData(Guid key, IPublishedContentType contentType) : base(key, contentType) { }

        public BlockListContentData(GridControl control, IPublishedContentType contentType) : base(control, contentType) { }

        public BlockListContentData<TModel> SetValue<TProperty>(Expression<Func<TModel, TProperty>> selector, object? value) {

            // Get the name/alias of the property
            string alias = ReflectionUtils.GetPropertyInfo(selector).Name;

            // Not sure how much casing matters, so we better lookup the correct casing of the property type
            IPublishedPropertyType? propertyType = ContentType.GetPropertyType(alias);
            if (propertyType is null) throw new Exception($"Property type with alias '{alias}' not found for content type '{ContentType.Alias}'.");

            // Set the property value
            SetValue(propertyType.Alias, value);

            return this;

        }

    }

}