using CORE.DTOs.APIs.Business;
using CORE.DTOs.APIs.MotorClaim;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MotorClaims.Models;
using Newtonsoft.Json;

namespace MotorClaims.Controllers
{
    public class SearchController : Controller
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string VehicleListCacheKey = "ServicesLink";
        private readonly IMemoryCache _memoryCache;
        public static IWebHostEnvironment _environment;
        CORE.DTOs.APIs.TP_Services.APIsLists query = new CORE.DTOs.APIs.TP_Services.APIsLists();
        public SearchController(IOptions<AppSettings> appSettings, IWebHostEnvironment environment, IMemoryCache memoryCache)
        {
            _environment = environment;
            _appSettings = appSettings.Value;
            _memoryCache = memoryCache;
            _memoryCache.TryGetValue(VehicleListCacheKey, out query);
        }
        public IActionResult Index()
        {
            ViewData["search"] = new Search();
            return View();
        }

        [HttpPost]
        public IActionResult Search(Search search)
        {
            SearchingObj searchingObj = new SearchingObj()
            {
                ChassisNo = search.chassis,
                ClaimNo = search.claimno,
                ComplainNo = search.complain,
                CustomNo = search.custom,
                MobileNo = search.mobile,
                NationalId = search.nationalid,
                PlateNo = search.plate,
                PolicyNo = search.policy,
                SequenceNo = search.sequence,
            };
            ClaimSearchResult claimSearchResult = new ClaimSearchResult();
            claimSearchResult = Helpers.ExcutePostAPI<ClaimSearchResult>(searchingObj, _appSettings.APIHubPrefix + "api/MotorClaim/SearchClaimInfo");
            //claimSearchResult = JsonConvert.DeserializeObject<ClaimSearchResult>(JsonConvert.SerializeObject(obj));
            ViewData["search"] = search;
            HttpContext.Session.SetSessionData("SearchResult", claimSearchResult);
            return View("Index", claimSearchResult);
        }
    }
}
