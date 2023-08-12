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
            return View();
        }
      
        [HttpPost]
        public IActionResult Search()
        {
            string Plate = HttpContext.Request.Form["plate"].ToString();
            string sequence = HttpContext.Request.Form["sequence"].ToString();
            string claimno = HttpContext.Request.Form["claimno"].ToString();
            string nationalid = HttpContext.Request.Form["nationalid"].ToString();
            string mobile = HttpContext.Request.Form["mobile"].ToString();
            string complain = HttpContext.Request.Form["complain"].ToString();
            string custom = HttpContext.Request.Form["custom"].ToString();
            string chassis = HttpContext.Request.Form["chassis"].ToString();
            string policyno = HttpContext.Request.Form["policy"].ToString();
            SearchingObj searchingObj = new SearchingObj()
            {
               ChassisNo = chassis,
                ClaimNo = claimno,
                 ComplainNo = complain,
                  CustomNo = custom,
                   MobileNo = mobile,
                    NationalId = nationalid,
                     PlateNo = Plate,
                      PolicyNo = policyno,
                       SequenceNo = sequence,
            };
            ClaimSearchResult claimSearchResult = new ClaimSearchResult();
            claimSearchResult = Helpers.ExcutePostAPI<ClaimSearchResult>(searchingObj, _appSettings.APIHubPrefix + "api/MotorClaim/SearchClaimInfo");
            //claimSearchResult = JsonConvert.DeserializeObject<ClaimSearchResult>(JsonConvert.SerializeObject(obj));
            return View("Index", claimSearchResult);
        }
    }
}
