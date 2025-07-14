using OptimizelyBlogs.Foundation.SiteSetting.Infrastructure;

namespace OptimizelyBlogs.Foundation.SiteSetting.Models
{
    [SettingsContentType(DisplayName = "Layout Settings",
        GUID = "1cf3cd40-1845-4601-a898-5732e6574559",
        Description = "Data sources and in future menu builder and other site configuration",
        AvailableInEditMode = true,
        SettingsName = "Layout Settings")]
    public class LayoutSettings : SettingsBase
    {
        public virtual int NumberOfHits {  get; set; }
    }
}
