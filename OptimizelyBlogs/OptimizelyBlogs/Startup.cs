using EPiServer.Cms.Shell;
using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using OptimizelyBlogs.Extensions;
using OptimizelyBlogs.Foundation.SiteSetting.Services.Implement;
using OptimizelyBlogs.Foundation.SiteSetting.Services.Interface;

namespace OptimizelyBlogs
{
    public class Startup
    {
        private readonly IWebHostEnvironment _webHostingEnvironment;

        public Startup(IWebHostEnvironment webHostingEnvironment)
        {
            _webHostingEnvironment = webHostingEnvironment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            if (_webHostingEnvironment.IsDevelopment())
            {
                AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(_webHostingEnvironment.ContentRootPath, "App_Data"));


                services.AddSingleton<ISettingsService, SettingsService>();

                services.Configure<SchedulerOptions>(options => options.Enabled = false);
            }

            services
                .AddCmsAspNetIdentity<ApplicationUser>()
                .AddCms()
                .AddAlloy()
                .AddAdminUserRegistration()
                .AddEmbeddedLocalization<Startup>();

            // Required by Wangkanai.Detection
            services.AddDetection();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Required by Wangkanai.Detection
            app.UseDetection();
            app.UseSession();

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapContent();
            });
        }
    }
}
