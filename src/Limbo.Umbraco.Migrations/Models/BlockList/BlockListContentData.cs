using System;
using System.Collections.Generic;
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
            Guid key = SecurityUtils.GetMd5Guid($"{control.Row.Id}#{index1}#{index2}");

            // Create an UDI based on the element type and the GUID key
            Udi = new GuidUdi("element", key);

            // Set the content type
            ContentType = contentType;

        }

        public BlockListContentData Add(string name, object? value) {
            Properties.Add(name, value);
            return this;
        }

    }

}