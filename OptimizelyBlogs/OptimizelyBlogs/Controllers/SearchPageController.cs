using Microsoft.AspNetCore.Mvc;
using OptimizelyBlogs.Foundation.SiteSetting.Models;
using OptimizelyBlogs.Foundation.SiteSetting.Services.Interface;
using OptimizelyBlogs.Models.Pages;
using OptimizelyBlogs.Models.ViewModels;

namespace OptimizelyBlogs.Controllers
{
    public class SearchPageController : PageControllerBase<SearchPage>
    {
        private ISettingsService _settingService {set; get; }
        public SearchPageController(ISettingsService settingsService) {
            _settingService = settingsService;
        }
        public ViewResult Index(SearchPage currentPage, string q)
        {

            var layoutsetting = _settingService.GetSiteSettings<LayoutSettings>();


            var model = new SearchContentModel(currentPage)
            {
                Hits = Enumerable.Empty<SearchContentModel.SearchHit>(),
                NumberOfHits = layoutsetting.NumberOfHits,
                SearchServiceDisabled = true,
                SearchedQuery = q
            };

            return View(model);
        }
    }
}
