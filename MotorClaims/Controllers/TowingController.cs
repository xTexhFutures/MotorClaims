using CORE.DTOs.APIs.MotorClaim;
using CORE.DTOs.Authentications;
using CORE.DTOs.MotorClaim.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MotorClaims.Models;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Engineering;
using System.Drawing;
using System.Security.Claims;
using X.PagedList;

namespace MotorClaims.Controllers
{
    public class TowingController : Controller
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string VehicleListCacheKey = "ServicesLink";
        private readonly IMemoryCache _memoryCache;
        public static IWebHostEnvironment _environment;
        CORE.DTOs.APIs.TP_Services.APIsLists query = new CORE.DTOs.APIs.TP_Services.APIsLists();

        public TowingController(IOptions<AppSettings> appSettings, IWebHostEnvironment environment, IMemoryCache memoryCache)
        {
            _environment = environment;
            _appSettings = appSettings.Value;
            _memoryCache = memoryCache;
            _memoryCache.TryGetValue(VehicleListCacheKey, out query);
        }

        public IActionResult Index(int page=1, string err=null)
        {
            ViewData["searchObj"] = new SearchObj();
            ViewData["Error"] = err;
            List<ClaimMaster> claim = new List<ClaimMaster>();
            claim = HttpContext.Session.getSessionData<List<ClaimMaster>>("ClaimMasterTowing");
            ViewData["AllUsers"] = HttpContext.Session.getSessionData<List<Users>>("AllUsers");
            IPagedList<ClaimMaster> claims = claim.ToPagedList(page, _appSettings.PageSize);
            return View(claims);
        }

        public IActionResult SearchTowings(SearchObj searchObj)
        {
            List<ClaimMaster> claim = new List<ClaimMaster>();
            IPagedList<ClaimMaster> Towings = claim.ToPagedList(1, _appSettings.PageSize);
            ViewData["searchObj"] = searchObj;
            ViewData["AllUsers"] = HttpContext.Session.getSessionData<List<Users>>("AllUsers");
            if (string.IsNullOrEmpty(searchObj.nationalid) && !searchObj.RegisteredFrom.HasValue && !searchObj.RegisteredTo.HasValue && string.IsNullOrEmpty(searchObj.chassis) && string.IsNullOrEmpty(searchObj.claimno) && string.IsNullOrEmpty(searchObj.mobile) && string.IsNullOrEmpty(searchObj.policy))
            {
                return RedirectToAction("Index", new { err = "Please fill at least one parameter" });
            }
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                NationalID = searchObj.nationalid,
                chassis = searchObj.chassis,
                claimno = searchObj.claimno,
                mobile = searchObj.mobile,
                policy = searchObj.policy,
                RegisteredFrom = searchObj.RegisteredFrom,
                RegisteredTo = searchObj.RegisteredTo

            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
                Request = mainSearchMC
            };
            claim = Helpers.ExcutePostAPI<List<ClaimMaster>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            HttpContext.Session.SetSessionData("ClaimMaster", claim);
            Towings = claim.ToPagedList(1, _appSettings.PageSize);
            return View("Index", Towings);
        }


        public IActionResult TowingLetter(int ClaimId,int ClaimantId)
        {
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = ClaimId,
                ClaimantId=ClaimantId
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
                Request = mainSearchMC
            };
            var claim = Helpers.ExcutePostAPI<List<ClaimMaster>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

             mainSearchMC = new MainSearchMC()
            {
                ClaimId = ClaimId,
                ClaimantId = ClaimantId
            };
             setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadTowings,
                Request = mainSearchMC
            };
            var towings = Helpers.ExcutePostAPI<List<Towings>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            ViewData["ClaimMaster"] = HttpContext.Session.getSessionData<List<ClaimMaster>>("ClaimMaster");
            ViewData["Cities"] = HttpContext.Session.getSessionData<List<LookupTable>>("Cities");
            ViewData["towings"] = towings!=null && towings.Count>0 ? towings.FirstOrDefault():new Towings();
            return View("_TowingLetter", claim.FirstOrDefault());
        }
        [HttpPost]
        public void UpdateTowing(Towings towings)
        {
            towings.CreationDate = DateTime.Now;
            towings.CreatedBy = HttpContext.Session.getSessionData<Users>("LoggedUser").UserName;
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertTowings,
                Request = towings
            };
            towings = Helpers.ExcutePostAPI<Towings>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            Helpers.RegisterHistory(_appSettings, towings.ClaimId, "Update Towing", HttpContext.Session.getSessionData<Users>("LoggedUser").UserName, towings.ClaimantId);


        }
    }
}

