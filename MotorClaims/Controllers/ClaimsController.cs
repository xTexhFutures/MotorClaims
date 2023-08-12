using CORE.DTOs.APIs.Business;
using CORE.DTOs.APIs.MotorClaim;
using CORE.DTOs.APIs.Process.Payments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MotorClaims.Models;
using System.Reflection;

namespace MotorClaims.Controllers
{
    public class ClaimsController : Controller
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string VehicleListCacheKey = "ServicesLink";
        private readonly IMemoryCache _memoryCache;
        public static IWebHostEnvironment _environment;
        CORE.DTOs.APIs.TP_Services.APIsLists query = new CORE.DTOs.APIs.TP_Services.APIsLists();

        public ClaimsController(IOptions<AppSettings> appSettings, IWebHostEnvironment environment, IMemoryCache memoryCache)
        {
            _environment = environment;
            _appSettings = appSettings.Value;
            _memoryCache = memoryCache;
            _memoryCache.TryGetValue(VehicleListCacheKey, out query);
        }
        public IActionResult Index(string? policyNo, string? chassis)
        {
            SearchingObj searchingObj = new SearchingObj()
            {
                ChassisNo = chassis,
             
                PolicyNo = policyNo
               
            };
            ClaimSearchResult claimSearchResult = new ClaimSearchResult();
            claimSearchResult = Helpers.ExcutePostAPI<ClaimSearchResult>(searchingObj, _appSettings.APIHubPrefix + "api/MotorClaim/SearchClaimInfo");

            

            return View(claimSearchResult);
        }

        public IActionResult SaveClaim()
        {
            string ReportType = HttpContext.Request.Form["vehicle_card"].ToString();
            string NajmRpt = HttpContext.Request.Form["NajmRpt"].ToString();
            string MroorRpt = HttpContext.Request.Form["MroorRpt"].ToString();
            string TaqdeerRpt = HttpContext.Request.Form["TaqdeerRpt"].ToString();
            string lossdate = HttpContext.Request.Form["lossdate"].ToString();
            string notificationdate = HttpContext.Request.Form["notificationdate"].ToString();
            string registrationdate = HttpContext.Request.Form["registrationdate"].ToString();
            string bank = HttpContext.Request.Form["bank"].ToString();
            string Iban = HttpContext.Request.Form["Iban"].ToString();
            string claimantname = HttpContext.Request.Form["claimantname"].ToString();
            string ClaimantSequence = HttpContext.Request.Form["ClaimantSequence"].ToString();
            string claimantChassis = HttpContext.Request.Form["claimantChassis"].ToString();
            string AccidentCity = HttpContext.Request.Form["AccidentCity"].ToString();
            string ClNatId = HttpContext.Request.Form["ClNatId"].ToString();
            string ownername = HttpContext.Request.Form["ownername"].ToString();
            string clMake = HttpContext.Request.Form["clMake"].ToString();
            string clModel = HttpContext.Request.Form["clModel"].ToString();
            string clcolor = HttpContext.Request.Form["clcolor"].ToString();

            string policy = HttpContext.Request.Form["policy"].ToString();
            string vehicle = HttpContext.Request.Form["vehicle"].ToString();
            string policyNo = HttpContext.Request.Form["policyNo"].ToString();
            string Effective = HttpContext.Request.Form["Effective"].ToString();
            string Expiry = HttpContext.Request.Form["Expiry"].ToString();
            string sequence = HttpContext.Request.Form["sequence"].ToString();
            string vehicleName = HttpContext.Request.Form["vehicleName"].ToString();

            string plate = HttpContext.Request.Form["plate"].ToString();
            string chassis = HttpContext.Request.Form["chassis"].ToString();
           

            return View();
        }
    }
}

