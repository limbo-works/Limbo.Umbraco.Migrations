﻿using System;
using System.Diagnostics.CodeAnalysis;
using Limbo.Umbraco.Migrations.Models.BlockList;
using Skybrud.Umbraco.GridData.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Limbo.Umbraco.Migrations.Services {

    public interface IMigrationsService {

        IContent? ImportContent(int id);

        IContent? ImportContent(Guid key);

        IMedia? ImportMedia(int id);

        IMedia? ImportMedia(Guid key);

        //public IMember ImportMember(int id);

        //public IMember ImportMember(Guid key);

        BlockListSettingsData? CreateDefaultBlockListSettings(GridControl control);

        BlockListContentData? CreateBlockListContentData<T>(Guid key) where T : PublishedElementModel;

        BlockListContentData CreateBlockListContentData<T>(GridControl control) where T : PublishedElementModel;

        BlockListSettingsData CreateBlockListSettingsData<T>(GridControl control) where T : PublishedElementModel;

        bool TryParseUdi(string value, [NotNullWhen(true)] out GuidUdi? result);

        string? ConvertRte(string text);

    }

}