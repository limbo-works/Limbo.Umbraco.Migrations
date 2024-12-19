using Limbo.Umbraco.Migrations.Constants;
using Limbo.Umbraco.Migrations.Models.UrlPicker;
using Limbo.Umbraco.MigrationsClient.Models.Content;
using Limbo.Umbraco.MigrationsClient.Models.Umbraco;
using Skybrud.Essentials.Exceptions;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

using UmbracoGuidUdi = Umbraco.Cms.Core.GuidUdi;

namespace Limbo.Umbraco.Migrations.Extensions;

public static class MigrationsExtensions {

    public static UmbracoGuidUdi? ToUmbracoUdi(this GuidUdi? udi) {
        return udi is null ? null : new UmbracoGuidUdi(udi.EntityType, udi.Guid);
    }

    public static UrlPickerList AddContent(this UrlPickerList list, LegacyContent content, string? target = null, string? queryString = null) {
        list.Add(UrlPickerItem.CreateContentItem(content.Name, new UmbracoGuidUdi(UmbracoEntityTypes.Content, content.Key), content.Url, target, queryString));
        return list;
    }

    public static UrlPickerList AddMedia(this UrlPickerList list, IMedia media, string? target = null, string? queryString = null) {
        string url = media.GetValue<string>("umbracoFile") ?? throw new BjernerSaysNoException();
        list.Add(UrlPickerItem.CreateMediaItem(media.Name!, media.GetUdi(), url, target, queryString));
        return list;
    }

    public static UrlPickerList? NullIfEmpty(this UrlPickerList? list) {
        return list is null || list.Count == 0 ? null : list;
    }

}