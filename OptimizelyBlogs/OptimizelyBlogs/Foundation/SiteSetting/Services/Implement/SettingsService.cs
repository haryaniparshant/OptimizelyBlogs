using EPiServer.Framework.Cache;
using EPiServer.Globalization;
using EPiServer.Logging;
using EPiServer.Web;
using OptimizelyBlogs.Foundation.SiteSetting.Models;
using OptimizelyBlogs.Foundation.SiteSetting.Services.Interface;
using System.Globalization;

namespace OptimizelyBlogs.Foundation.SiteSetting.Services.Implement
{
    public class SettingsService : ISettingsService
    {
        private readonly IContentRepository _contentRepository;
        private readonly IContentVersionRepository _contentVersionRepository;
        private readonly ContentRootService _contentRootService;
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly EPiServer.Logging.ILogger _log = LogManager.GetLogger();
        private readonly IContextModeResolver _contextModeResolver;
        private readonly ISynchronizedObjectInstanceCache _objectCache;
        private readonly IContentCacheKeyCreator _contentCacheKeyCreator;
        private const string StartPageType = "StartPage";
        private const string SettingsFolderPropName = "SettingsFolder";
        private const string SiteSettingsCachePrefix = "SiteSettingsCache";


        public SettingsService(
            IContentRepository contentRepository,
            IContentVersionRepository contentVersionRepository,
            ContentRootService contentRootService,
            IContentTypeRepository contentTypeRepository,
            IContextModeResolver contextModeResolver,
            ISynchronizedObjectInstanceCache objectCache,
            IContentCacheKeyCreator contentCacheKeyCreator)
        {
            _contentRepository = contentRepository;
            _contentVersionRepository = contentVersionRepository;
            _contentRootService = contentRootService;
            _contentTypeRepository = contentTypeRepository;
            _contextModeResolver = contextModeResolver;
            _objectCache = objectCache;
            _contentCacheKeyCreator = contentCacheKeyCreator;
        }

        public ContentReference? GlobalSettingsRoot { get; set; }

        public string GetSiteSettingsCacheKey(ContentReference startPageRef, Type settingsType, bool isDraft = false, string contentLanguage = "")
        {
            var draftSegment = isDraft ? "-common-draft" : string.Empty;
            return $"{SiteSettingsCachePrefix}-{startPageRef.ID}-{draftSegment}-{contentLanguage}-{settingsType.Name}";
        }

        public T? GetSiteSettings<T>(ContentReference startPageReference, string language = "") where T : SettingsBase
        {
            try
            {
                var contentLanguage = string.IsNullOrEmpty(language) ? ContentLanguage.PreferredCulture.Name : language;
                var isDraft = _contextModeResolver.CurrentMode == ContextMode.Edit;
                var cacheKey = GetSiteSettingsCacheKey(startPageReference, typeof(T), isDraft, contentLanguage);
                var settings = _objectCache.Get(cacheKey) as T;
                if (settings != null)
                    return settings;

                settings = LoadSettings<T>(startPageReference, isDraft, contentLanguage);

                if (settings != null)
                {
                    var evictionPolicy = isDraft ?
                    new CacheEvictionPolicy(new string[]
                        {
                            _contentCacheKeyCreator.CreateVersionCommonCacheKey(startPageReference),
                            _contentCacheKeyCreator.CreateVersionCommonCacheKey(settings.ContentLink)
                        })
                    : new CacheEvictionPolicy(new string[]
                        {
                            _contentCacheKeyCreator.CreateCommonCacheKey(startPageReference),
                            _contentCacheKeyCreator.CreateCommonCacheKey(settings.ContentLink)
                        });

                    _objectCache.Insert(cacheKey, settings, evictionPolicy);
                }

                return settings;
            }
            catch (ArgumentNullException argumentNullException)
            {
                _log.Error($"SettingsService : GetSiteSettings argumentNullException", exception: argumentNullException);
            }
            catch (NullReferenceException nullReferenceException)
            {
                _log.Error($"SettingsService : GetSiteSetting nullReferenceException", exception: nullReferenceException);
            }
            return default;
        }

        public T GetSiteSettings<T>() where T : SettingsBase
        {
            return this.GetSiteSettings<T>(
                SiteDefinition.Current.StartPage,
                CultureInfo.CurrentCulture.ToString());
        }

        public void InitializeSettings()
        {
            try
            {
                RegisterContentRoots();
            }
            catch (NotSupportedException notSupportedException)
            {
                _log.Error($"SettingsService : InitializeSettings ", exception: notSupportedException);
            }
        }

        public T? LoadSettings<T>(ContentReference startPageRef, bool isDraft, string language) where T : SettingsBase
        {
            if (startPageRef != null && startPageRef.ID > 0)
            {
                try
                {
                    var startPage = _contentRepository.Get<IContent>(startPageRef, new CultureInfo(language)) ??
                                    _contentRepository.Get<IContent>(startPageRef);
                    if (startPage != null)
                    {
                        var settingsFolderRef = startPage.Property[SettingsFolderPropName].Value as ContentReference;
                        if (settingsFolderRef != null)
                        {
                            var settings = _contentRepository.GetChildren<T>(settingsFolderRef).FirstOrDefault();
                            if (settings != null)
                            {
                                if (isDraft)
                                {
                                    var draftContentLink = _contentVersionRepository.LoadCommonDraft(settings.ContentLink, language);
                                    if (draftContentLink != null)
                                    {
                                        var settingsDraft = _contentRepository.Get<T>(draftContentLink.ContentLink);
                                        return settingsDraft;
                                    }
                                }

                                if (settings.ExistingLanguages.Any(t => string.Equals(t.Name, language, StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    return _contentRepository.Get<T>(settings.ContentLink, new CultureInfo(language));
                                }

                                return settings;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error($"SettingsService : LoadSettings ", exception: ex);
                }
            }

            return null;
        }

        private void RegisterContentRoots()
        {
            var registeredRoots = _contentRepository.GetItems(_contentRootService.List(), new LoaderOptions());
            var settingsRootRegistered = registeredRoots.Any(x => x.ContentGuid == SettingsFolder.SettingsRootGuid && x.Name.Equals(SettingsFolder.SettingsRootName));

            if (!settingsRootRegistered)
            {
                _contentRootService.Register<SettingsFolder>(SettingsFolder.SettingsRootName, SettingsFolder.SettingsRootGuid, ContentReference.RootPage);
            }

            var root = _contentRepository.GetItems(_contentRootService.List(), new LoaderOptions())
                 .FirstOrDefault(x => x.ContentGuid == SettingsFolder.SettingsRootGuid);

            if (root == null)
                return;

            GlobalSettingsRoot = root.ContentLink;
        }

        /// <summary>
        /// Resolve the start page from a page reference (the highest order ancestor which is of type Start Page)
        /// </summary>
        /// <param name="pageReference"></param>
        /// <returns></returns>
        public ContentReference? ResolveStartPageByReference(ContentReference pageReference)
        {
            var startPageType = _contentTypeRepository.Load(StartPageType);
            if (_contentRepository.TryGet<PageData>(pageReference, out var page))
            {
                if (page != null)
                {
                    if (Equals(page.ParentLink, ContentReference.RootPage) && page.ContentTypeID == startPageType.ID)
                        return pageReference;

                    var ancestors = _contentRepository.GetAncestors(pageReference);
                    if (ancestors.Count() < 2)
                        return null;

                    var startPage = ancestors.ElementAt(ancestors.Count() - 2);
                    if (startPage.ContentTypeID == startPageType.ID)
                        return startPage.ContentLink;
                }
            }

            return null;
        }

        /// <summary>
        /// Resolve the start page from a page pageGuid (the highest order ancestor which is of type Start Page)
        /// </summary>
        /// <param name="pageGuid"></param>
        /// <returns></returns>
        public ContentReference? ResolveStartPageByGuid(Guid pageGuid)
        {
            if (_contentRepository.TryGet<PageData>(pageGuid, out var page))
            {
                return ResolveStartPageByReference(page.ContentLink);
            }

            return null;
        }
    }
}
