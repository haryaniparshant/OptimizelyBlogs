using OptimizelyBlogs.Foundation.SiteSetting.Models;

namespace OptimizelyBlogs.Foundation.SiteSetting.Services.Interface
{
    public interface ISettingsService
    {
        ContentReference? GlobalSettingsRoot { get; set; }
        T? GetSiteSettings<T>(ContentReference startPageReference, string language = "") where T : SettingsBase;

        /// <summary>
        /// Gets the Settings of type
        /// </summary>
        /// <typeparam name="T">T type</typeparam>
        /// <returns>T type of settings</returns>
        T GetSiteSettings<T>() where T : SettingsBase;

        void InitializeSettings();

        /// <summary>
        /// Resolve the start page from a page reference (the highest order ancestor which is of type Start Page)
        /// </summary>
        /// <param name="pageReference"></param>
        /// <returns></returns>
        ContentReference? ResolveStartPageByReference(ContentReference pageReference);

        /// <summary>
        /// Resolve the start page from a page pageGuid (the highest order ancestor which is of type Start Page)
        /// </summary>
        /// <param name="pageGuid"></param>
        /// <returns></returns>
        ContentReference? ResolveStartPageByGuid(Guid pageGuid);
    }
}
