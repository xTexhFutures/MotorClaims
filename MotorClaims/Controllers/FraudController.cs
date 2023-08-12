using CORE.DTOs.APIs.Business;
using CORE.DTOs.APIs.MotorClaim;
using CORE.DTOs.MotorClaim.Frauds;
using CORE.DTOs.MotorClaim.WorkFlow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MotorClaims.Models;

namespace MotorClaims.Controllers
{
    public class FraudController : Controller
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string VehicleListCacheKey = "ServicesLink";
        private readonly IMemoryCache _memoryCache;
        public static IWebHostEnvironment _environment;
        CORE.DTOs.APIs.TP_Services.APIsLists query = new CORE.DTOs.APIs.TP_Services.APIsLists();
        public FraudController(IOptions<AppSettings> appSettings, IWebHostEnvironment environment, IMemoryCache memoryCache)
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
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadFraudSetup,
                Request = mainSearchMC
            };
            var FraudSetup = Helpers.ExcutePostAPI<List<FraudSetup>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            ViewData["FraudSetup"] = FraudSetup;
            mainSearchMC = new MainSearchMC();
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadFraudIndicator,
                Request = mainSearchMC
            };
            var FraudIndicators = Helpers.ExcutePostAPI<List<FraudIndicators>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            ViewData["FraudIndicators"] = FraudIndicators;
            return View();
        }

        public IActionResult UpdateFraudIndicator(int Id)
        {
            MainSearchMC mainSearchMC = new MainSearchMC();
            List<FraudIndicators> fraudIndicators = new List<FraudIndicators>();
            if (Id > 0)
            {
                mainSearchMC = new MainSearchMC()
                {
                    Id = Id
                };
                SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.LoadFraudIndicator,
                    Request = mainSearchMC
                };
                fraudIndicators = Helpers.ExcutePostAPI<List<FraudIndicators>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            }


            return View(Id > 0 ? fraudIndicators.FirstOrDefault() : new FraudIndicators());
        }

        public IActionResult UpdateFraudSetup(int Id)
        {
            MainSearchMC mainSearchMC = new MainSearchMC();
            List<FraudSetup> fraudSetups = new List<FraudSetup>();
            if (Id > 0)
            {
                mainSearchMC = new MainSearchMC()
                {
                    Id = Id
                };
                SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.LoadFraudSetup,
                    Request = mainSearchMC
                };
                fraudSetups = Helpers.ExcutePostAPI<List<FraudSetup>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            }


            return View(Id > 0 ? fraudSetups.FirstOrDefault() : new FraudSetup());
        }


        [HttpPost]
        public IActionResult UpdateFraudSetup(FraudSetup fraudSetup)
        {

            if (fraudSetup.ScoreTo < fraudSetup.ScoreFrom)
            {
                return RedirectToAction("Index", new { err = "ScoreFrom should be Less than Score To" });
            }

            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                ScoreFrom = fraudSetup.ScoreFrom,
                ScoreTo = fraudSetup.ScoreTo,
                Id = fraudSetup.Id,
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadFraudSetup,
                Request = mainSearchMC
            };
            var FraudSetup = Helpers.ExcutePostAPI<List<FraudSetup>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            if (FraudSetup.Count > 0)
            {
                return RedirectToAction("Index", new {err= "There are overlap on Score" });
            }
            fraudSetup.CreatedBy = "Test";
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertFraudSetup,
                Request = fraudSetup
            };
            fraudSetup = Helpers.ExcutePostAPI<FraudSetup>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult UpdateFraudIndicator(FraudIndicators fraudIndicators)
        {
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Name = fraudIndicators.Name
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadFraudIndicator,
                Request = mainSearchMC
            };
            var FraudSetup = Helpers.ExcutePostAPI<List<FraudIndicators>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            if (FraudSetup.Count > 0)
            {
                return RedirectToAction("Index", new { err = "This Indicator Name already exist" });
            }

            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateFraudIndicator,
                Request = fraudIndicators
            };
            var Fraud = Helpers.ExcutePostAPI<FraudIndicators>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            return RedirectToAction("Index");
        }


        public IActionResult DeleteFraud(int Id)
        {
            ViewData["Controller"] = "Fraud";
            ViewData["Action"] = "DeleteFraudPost";

            return View("_DeleteConfirmation", Id);
        }

        [HttpPost]
        public IActionResult DeleteFraudPost(int Id)
        {
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = Id
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.DeleteFraudIndicator,
                Request = mainSearchMC
            };
            var Fraud = Helpers.ExcutePostAPI<bool>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult SearchFrauds()
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
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadFraudIndicator,
                Request = mainSearchMC
            };
            var Frauds = Helpers.ExcutePostAPI<List<FraudIndicators>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");
            ViewData["FraudIndicators"] = Frauds;
            ViewData["Error"] = null;
            ViewData["Filter"] = Status;
            return View("Index");
        }


    }
}
