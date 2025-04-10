using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Limbo.Umbraco.Migrations.Models;
using Limbo.Umbraco.Migrations.Models.BlockList;
using Limbo.Umbraco.Migrations.Models.Rte;
using Limbo.Umbraco.Migrations.Models.UrlPicker;
using Limbo.Umbraco.MigrationsClient.Models.Skybrud.Grid;
using Limbo.Umbraco.MigrationsClient.Models.Skybrud.LinkPicker;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Limbo.Umbraco.Migrations.Services;

public interface IMigrationsService {

    ContentImportResult ImportContent(int id);

    ContentImportResult ImportContent(Guid key);

    IMedia? ImportMedia(int id);

    IMedia? ImportMedia(Guid key);

    //public IMember ImportMember(int id);

    //public IMember ImportMember(Guid key);

    BlockListSettingsData? CreateDefaultBlockListSettings(GridControl control);

    BlockListContentData<T> CreateBlockListContentData<T>(Guid key) where T : PublishedElementModel;

    BlockListContentData<T> CreateBlockListContentData<T>(GridControl control) where T : PublishedElementModel;

    BlockListSettingsData<T> CreateBlockListSettingsData<T>(Guid key) where T : PublishedElementModel;

    BlockListSettingsData<T> CreateBlockListSettingsData<T>(GridControl control) where T : PublishedElementModel;

    GuidUdi ParseGuidUdi(string value);

    bool TryParseUdi(string? value, [NotNullWhen(true)] out GuidUdi? result);

    RteModel? ConvertRte(string? input, out IReadOnlyList<Warning> warnings);

    UrlPickerList? ConvertLinkPickerList(LinkPickerList? list);

    UrlPickerItem? ConvertLinkPickerItem(LinkPickerItem? item);

    UrlPickerList? ConvertLinkPickerItemAsList(LinkPickerItem? item);

    JToken ToJson(object value);

}