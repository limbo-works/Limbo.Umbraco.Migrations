using System.Linq;
using Limbo.Umbraco.Migrations.Constants;
using Limbo.Umbraco.Migrations.Exceptions;
using Limbo.Umbraco.Migrations.Models.MediaPicker;
using Limbo.Umbraco.Migrations.Models.UrlPicker;
using Limbo.Umbraco.MigrationsClient.Models.Content;
using Limbo.Umbraco.MigrationsClient.Models.Umbraco;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

using UmbracoGuidUdi = Umbraco.Cms.Core.GuidUdi;

namespace Limbo.Umbraco.Migrations.Extensions;

public static class MigrationsExtensions {

    public static UmbracoGuidUdi? ToUmbracoUdi(this GuidUdi? udi) {
        return udi is null ? null : new UmbracoGuidUdi(udi.EntityType, udi.Guid);
    }


    /// <summary>
    /// Adds a new content item based on the specified <paramref name="content"/>.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="content">The content item the list should be based.</param>
    /// <param name="target">The target of the URL item. Default is <see langword="null"/>.</param>
    /// <param name="queryString">The query string, if any.</param>
    /// <returns><paramref name="list"/> - useful for method chaining.</returns>
    public static TList AddContent<TList>(this TList list, LegacyContent content, string? target = null, string? queryString = null) where TList : UrlPickerList {
        list.Add(UrlPickerItem.CreateContentItem(content.Name, new UmbracoGuidUdi(UmbracoEntityTypes.Content, content.Key), content.Url, target, queryString));
        return list;
    }

    /// <summary>
    /// Adds a new media item based on the specified <paramref name="media"/>.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="media">The media item the list should be based.</param>
    /// <param name="target">The target of the URL item. Default is <see langword="null"/>.</param>
    /// <param name="queryString">The query string, if any.</param>
    /// <returns><paramref name="list"/> - useful for method chaining.</returns>
    public static TList AddMedia<TList>(this TList list, IMedia media, string? target = null, string? queryString = null) where TList : UrlPickerList {
        // TODO: can we get the URL this way for images? (value might be JSON)
        string url = media.GetValue<string>("umbracoFile") ?? throw new MigrationsException($"Failed determening URL for media with key '{media.Key}'.");
        list.Add(UrlPickerItem.CreateMediaItem(media.Name!, media.GetUdi(), url, target, queryString));
        return list;
    }

    /// <summary>
    /// Adds a new external item based on the specified <paramref name="url"/>.
    /// </summary>
    /// <typeparam name="TList"></typeparam>
    /// <param name="list"></param>
    /// <param name="url">The URL the item should be based on.</param>
    /// <param name="name">The name of the item. If not specified, the name will be the same as <paramref name="url"/>.</param>
    /// <param name="target">The target of the URL item. Default is <c>_blank</c>.</param>
    /// <param name="queryString">The query string, if any.</param>
    /// <returns><paramref name="list"/> - useful for method chaining.</returns>
    public static TList AddExternal<TList>(this TList list, string url, string? name = null, string? target = "_blank", string? queryString = null) where TList : UrlPickerList {
        list.Add(new UrlPickerItem(LinkType.External, name ?? url, null, url, target, queryString));
        return list;
    }

    /// <summary>
    /// Returns <see langword="null"/> if <paramref name="list"/> is also <see langword="null"/>, otherwise, <paramref name="list"/> is returned unmodified.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <returns><paramref name="list"/> if not <see langword="null"/>; otherwise, <see langword="null"/>.</returns>
    public static TList? NullIfEmpty<TList>(this TList? list) where TList : UrlPickerList {
        return list is null || list.Count == 0 ? null : list;
    }

    /// <summary>
    /// Returns <see langword="null"/> if <paramref name="list"/> is also <see langword="null"/>, otherwise, <paramref name="list"/> is returned unmodified.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <returns><paramref name="list"/> if not <see langword="null"/>; otherwise, <see langword="null"/>.</returns>
    public static MediaPickerList? NullIfEmpty(this MediaPickerList? list) {
        return list is null || list.Count == 0 ? null : list;
    }

    /// <summary>
    /// Returns an instance of <see cref="ILegacyContentItem"/> representing the parent node of <paramref name="content"/>, or <see langword="null"/> if <paramref name="content"/> doesn't have a parent.
    /// </summary>
    /// <param name="content">The content node to get the parent for.</param>
    /// <returns>An instance of <see cref="ILegacyContentItem"/> if successful; otherwise, <see langword="null"/>.</returns>
    public static ILegacyContentItem? GetParent(this ILegacyContent content) {
        return content.Path.LastOrDefault();
    }

}