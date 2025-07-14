using EPiServer.Core.Internal;
using EPiServer.Shell;
using EPiServer.Shell.ViewComposition;
using OptimizelyBlogs.Foundation.SiteSetting.Infrastructure;

namespace OptimizelyBlogs.Foundation.SiteSetting.Components
{
    [Component]
    public class GlobalSettingsComponent : ComponentDefinitionBase
    {
        public GlobalSettingsComponent()
           : base("epi-cms/component/MainNavigationComponent")
        {
            LanguagePath = "/episerver/cms/components/globalsettings";
            base.Title = "Site settings";
            SortOrder = 5000;
            PlugInAreas = new[] { PlugInArea.AssetsDefaultGroup };
            base.Settings.Add(new Setting("repositoryKey", value: GlobalSettingsRepositoryDescriptor.RepositoryKey));
        }
    }
}
