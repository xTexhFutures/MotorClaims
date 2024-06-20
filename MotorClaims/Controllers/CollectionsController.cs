using CORE.DTOs.APIs.MotorClaim;
using CORE.DTOs.Authentications;
using CORE.DTOs.MotorClaim.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MotorClaims.Models;
using System.Collections.Generic;
using X.PagedList;

namespace MotorClaims.Controllers
{
    public class CollectionsController : Controller
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string LookupTable = "LookupTable";
        private readonly IMemoryCache _memoryCache;
        public static IWebHostEnvironment _environment;
        List<LookupTable> query = new List<LookupTable>();

        public CollectionsController(IOptions<AppSettings> appSettings, IWebHostEnvironment environment, IMemoryCache memoryCache)
        {
            _environment = environment;
            _appSettings = appSettings.Value;
            _memoryCache = memoryCache;
            _memoryCache.TryGetValue(LookupTable, out query);
        }
        
                    [CustomAuthorize(Roles = "Admin,Opertion,Collections")]
        public IActionResult Index(int page = 1, string? err = null)
        {
            ViewData["searchObj"] = new SearchObj();
            ViewData["Error"] = err;
            List<ClaimMaster> claim = new List<ClaimMaster>();
            claim = HttpContext.Session.getSessionData<List<ClaimMaster>>("ClaimMaster");
            ViewData["AllUsers"] = HttpContext.Session.getSessionData<List<Users>>("AllUsers");
            IPagedList<ClaimMaster> claims = claim.ToPagedList(page, _appSettings.PageSize);
            return View(claims);
        }
        [HttpPost]
        public IActionResult SearchRecoveries(SearchObj searchObj)
        {
            List<ClaimMaster> claim = new List<ClaimMaster>();
            IPagedList<ClaimMaster> Operations = claim.ToPagedList(1, _appSettings.PageSize);
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
                RegisteredTo = searchObj.RegisteredTo,
                IsRecovery = true

            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
                Request = mainSearchMC
            };
            claim = Helpers.ExcutePostAPI<List<ClaimMaster>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            HttpContext.Session.SetSessionData("ClaimMaster", claim);
            Operations = claim.ToPagedList(1, _appSettings.PageSize);
            return View("Index", Operations);
        }

        public IActionResult RegisterCollection(string obj)
        {
            ClaimSearchobj claimSearchobj = Helpers.Deserilize<ClaimSearchobj>(Helpers.Decryption(obj));
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = Convert.ToInt32(claimSearchobj.ClaimId),
                ClaimantId = claimSearchobj.ClaimantId

            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
                Request = mainSearchMC
            };
            var claim = Helpers.ExcutePostAPI<List<ClaimMaster>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            ViewData["query"] = query;
            ViewData["obj"] = obj;
            mainSearchMC = new MainSearchMC()
            {
                ClaimId = Convert.ToInt32(claimSearchobj.ClaimId),
                ClaimantId = claimSearchobj.ClaimantId

            };
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimRecoveries,
                Request = mainSearchMC
            };
            var Recoveries = Helpers.ExcutePostAPI<List<ClaimRecoveries>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            ViewData["Recoveries"] = Recoveries;
            HttpContext.Session.SetSessionData("Recoveries", Recoveries);
            HttpContext.Session.SetSessionData("RecoveryObj", obj);
            return View(claim.FirstOrDefault());
        }



        public IActionResult CollectRecoveries(string Id)
        {
            ClaimSearchobj claimSearchobj = Helpers.Deserilize<ClaimSearchobj>(Helpers.Decryption(Id));
            List<ClaimRecoveries> claimRecoveries = new List<ClaimRecoveries>();
            List<ClaimTransactions> collections =new List<ClaimTransactions>();
            string FinalIds = string.Empty;
            //string[] strings = Ids.Split(',');
            //string[] Id;
            //foreach (string str in strings)
            //{
            //    Id=str.Split("_");
            //    if (Id.Count<string>()>1)
            //    {
            //        FinalIds += Id[1] + ",";
            //        claimRecovery1 = claimRecoveries.Where(p => p.Id == Convert.ToInt32(Id[1])).FirstOrDefault();
            //        claimRecovery.ClaimantId = claimRecovery1.ClaimantId;
            //        claimRecovery.ClaimId = claimRecovery1.ClaimId;
            //        claimRecovery.RecoveryAmount += claimRecovery1.RecoveryAmount;

            //    }

            //}

            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = claimSearchobj.Id,
                ClaimantId= claimSearchobj.ClaimantId,
                ClaimId=Convert.ToInt32(claimSearchobj.ClaimId)

            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimRecoveries,
                Request = mainSearchMC
            };
            claimRecoveries = Helpers.ExcutePostAPI<List<ClaimRecoveries>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");


            mainSearchMC = new MainSearchMC()
            {
                TransactionType=(int)Enums.ClaimTransactionTypes.Collection,
                ClaimantId= claimSearchobj.ClaimantId,
                TransactionId = claimRecoveries.FirstOrDefault().ClaimTransactionId

            };
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimTransactions,
                Request = mainSearchMC
            };
            collections = Helpers.ExcutePostAPI<List<ClaimTransactions>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            ViewData["collections"] = collections;
            return PartialView("_CollectRecoveries", claimRecoveries.FirstOrDefault());
        }
    }
}
