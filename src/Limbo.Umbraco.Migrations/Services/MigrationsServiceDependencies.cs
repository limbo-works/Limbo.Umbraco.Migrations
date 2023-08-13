using Limbo.Umbraco.Migrations.Converters.Grid;
using Limbo.Umbraco.Migrations.Converters.Properties;
using Limbo.Umbraco.MigrationsClient;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Skybrud.Umbraco.GridData.Factories;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Limbo.Umbraco.Migrations.Services {

    public class MigrationsServiceDependencies {

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment WebHostEnvironment { get; }
        public IContentService ContentService { get; }
        public IMediaService MediaService { get; }
        public IMemberService MemberService { get; }
        public MediaFileManager MediaFileManager { get; }
        public MediaUrlGeneratorCollection MediaUrlGeneratorCollection { get; }
        public IShortStringHelper ShortStringHelper { get; }
        public PropertyConverterCollection PropertyConverterCollection { get; }
        public IContentTypeBaseServiceProvider ContentTypeBaseServiceProvider { get; }
        public IGridFactory GridFactory { get; }
        public GridControlConverterCollection GridControlConverters { get; }
        public IMigrationsClient MigrationsClient { get; }

        public MigrationsServiceDependencies(IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment,
            IContentService contentService,
            IMediaService mediaService,
            IMemberService memberService,
            MediaFileManager mediaFileManager,
            MediaUrlGeneratorCollection mediaUrlGeneratorCollection,
            IShortStringHelper shortStringHelper,
            PropertyConverterCollection propertyConverterCollection,
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            IGridFactory gridFactory,
            GridControlConverterCollection gridControlConverters,
            IMigrationsClient migrationsClient) {
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
            ContentService = contentService;
            MediaService = mediaService;
            MemberService = memberService;
            MediaFileManager = mediaFileManager;
            MediaUrlGeneratorCollection = mediaUrlGeneratorCollection;
            ShortStringHelper = shortStringHelper;
            PropertyConverterCollection = propertyConverterCollection;
            ContentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
            GridFactory = gridFactory;
            GridControlConverters = gridControlConverters;
            MigrationsClient = migrationsClient;
        }

    }

}