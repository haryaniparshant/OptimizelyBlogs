using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using OptimizelyBlogs.Foundation.SiteSetting.Models;
using OptimizelyBlogs.Foundation.SiteSetting.Services.Interface;

namespace OptimizelyBlogs.Foundation.SiteSetting.Infrastructure
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class SettingsInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            var settingsService = context.Locate.Advanced.GetInstance<ISettingsService>();
            settingsService.InitializeSettings();
        }


        public void Uninitialize(InitializationEngine context) { }
    }
}
