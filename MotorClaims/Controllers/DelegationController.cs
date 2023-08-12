using CORE.DTOs.APIs.Business;
using CORE.DTOs.APIs.MotorClaim;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MotorClaims.Models;
using CORE.DTOs.MotorClaim.WorkFlow;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Engineering;

namespace MotorClaims.Controllers
{
    public class DelegationController : Controller
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string VehicleListCacheKey = "ServicesLink";
        private readonly IMemoryCache _memoryCache;
        public static IWebHostEnvironment _environment;
        CORE.DTOs.APIs.TP_Services.APIsLists query = new CORE.DTOs.APIs.TP_Services.APIsLists();
        public DelegationController(IOptions<AppSettings> appSettings, IWebHostEnvironment environment, IMemoryCache memoryCache)
        {
            _environment = environment;
            _appSettings = appSettings.Value;
            _memoryCache = memoryCache;
            _memoryCache.TryGetValue(VehicleListCacheKey, out query);
        }



        public IActionResult Index(string err = null, int? Status = null)
        {
            ViewData["Error"] = err;
            ViewData["Filter"] = Status;
            MainSearchMC mainSearchMC = new MainSearchMC();
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadDelegation,
                Request = mainSearchMC
            };
            var Delegations = Helpers.ExcutePostAPI<List<DelegationSetup>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");
            ViewData["Delegations"] = Delegations;
            return View();
        }
        public IActionResult UpdateDelegation(int Id)
        {
            if (Id > 0)
            {
                MainSearchMC mainSearchMC = new MainSearchMC()
                {
                    Id = Id
                };
                SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.LoadDelegation,
                    Request = mainSearchMC
                };
                var Delegations = Helpers.ExcutePostAPI<List<DelegationSetup>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

                return View(Delegations.FirstOrDefault());
            }
            return View(new DelegationSetup());
        }

        [HttpPost]
        public IActionResult UpdateDelegation(DelegationSetup delegationSetup)
        {
            if (delegationSetup.To < delegationSetup.From)
            {
                return RedirectToAction("Index", new { err = "From Should be less than To" });
            }
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateDelegation,
                Request = delegationSetup
            };
            var Delegations = Helpers.ExcutePostAPI<DelegationSetup>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult SearchDelegations()
        {
            int? Status = null;
            try
            {
                Status = Convert.ToInt32(HttpContext.Request.Form["Filter"]);
            }
            catch (Exception)
            {
            }


            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Status = Status
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadDelegation,
                Request = mainSearchMC
            };
            var Delegations = Helpers.ExcutePostAPI<List<DelegationSetup>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");
            ViewData["Delegations"] = Delegations;
            ViewData["Error"] = null;
            ViewData["Filter"] = Status;
            return View("Index");
        }


        public IActionResult DeleteDelegation(int Id)
        {
            ViewData["Controller"] = "Delegation";
            ViewData["Action"] = "DeleteDelegationPost";

            return View("_DeleteConfirmation",Id);
        }

        [HttpPost]
        public IActionResult DeleteDelegationPost(int Id)
        {
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = Id
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.DeleteDelegation,
                Request = mainSearchMC
            };
            var Delegations = Helpers.ExcutePostAPI<bool>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            return RedirectToAction("Index");
        }



    

        
    }
}
