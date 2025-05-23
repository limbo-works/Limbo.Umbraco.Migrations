using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Limbo.Umbraco.Migrations.Extensions;
using Limbo.Umbraco.MigrationsClient.Models.Content;
using Umbraco.Cms.Core.Models;

namespace Limbo.Umbraco.Migrations.Models.UrlPicker;

/// <summary>
/// Class representing a URL picker list.
/// </summary>
public class UrlPickerList : IReadOnlyList<UrlPickerItem> {

    private readonly List<UrlPickerItem> _items = [];

    #region Properties

    public int Count => _items.Count;

    public UrlPickerItem this[int index] => _items[index];

    #endregion

    #region Constructors

    public UrlPickerList() { }

    public UrlPickerList(UrlPickerItem item) {
        Add(item);
    }

    public UrlPickerList(IEnumerable<UrlPickerItem> items) {
        _items.AddRange(items);
    }

    #endregion

    #region Member methods

    public void Add(UrlPickerItem item) {
        _items.Add(item);
    }

    public IEnumerator<UrlPickerItem> GetEnumerator() {
        return _items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    #endregion

    #region Static methods

    /// <summary>
    /// Creates and returns a new <see cref="UrlPickerList"/> instance containing an item for the specified <paramref name="content"/>.
    /// </summary>
    /// <param name="content">The content item the list should be based.</param>
    /// <param name="target">The target of the URL item. Default is <see langword="null"/>.</param>
    /// <param name="queryString">The query string, if any.</param>
    /// <returns>An instance of <see cref="UrlPickerList"/>, or <see langword="null"/> if <paramref name="content"/> is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(content))]
    public static UrlPickerList? CreateFromContent(LegacyContent? content, string? target = null, string? queryString = null) {
        return content is null ? null : new UrlPickerList().AddContent(content, target, queryString);
    }

    /// <summary>
    /// Creates and returns a new <see cref="UrlPickerList"/> instance containing an item for the specified <paramref name="media"/>.
    /// </summary>
    /// <param name="media">The content item the list should be based.</param>
    /// <param name="target">The target of the URL item. Default is <see langword="null"/>.</param>
    /// <param name="queryString">The query string, if any.</param>
    /// <returns>An instance of <see cref="UrlPickerList"/>, or <see langword="null"/> if <paramref name="media"/> is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(media))]
    public static UrlPickerList? CreateFromMedia(IMedia? media, string? target = null, string? queryString = null) {
        return media is null ? null : new UrlPickerList().AddMedia(media, target, queryString);
    }

    /// <summary>
    /// Creates and returns a new <see cref="UrlPickerList"/> instance containing an external item for the specified <paramref name="url"/>.
    /// </summary>
    /// <param name="url">The external URL item the list should be based.</param>
    /// <param name="name">The name of the item. If not specified, the name will be the same as <paramref name="url"/>.</param>
    /// <param name="target">The target of the URL item. Default is <c>_blank</c>.</param>
    /// <param name="queryString">The query string, if any.</param>
    /// <returns>An instance of <see cref="UrlPickerList"/>, or <see langword="null"/> if <paramref name="url"/> is <see langword="null"/> or white space.</returns>
    [return: NotNullIfNotNull(nameof(url))]
    public static UrlPickerList? CreateFromExternal(string? url, string? name = null, string? target = "_blank", string? queryString = null) {
        return string.IsNullOrWhiteSpace(url) ? null : new UrlPickerList().AddExternal(url, name, target, queryString);
    }

    #endregion

}