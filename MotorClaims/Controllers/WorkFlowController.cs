using CORE.DTOs.APIs.MotorClaim;
using CORE.DTOs.MotorClaim.WorkFlow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MotorClaims.Models;

namespace MotorClaims.Controllers
{
    public class WorkFlowController : Controller
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string VehicleListCacheKey = "ServicesLink";
        private readonly IMemoryCache _memoryCache;
        public static IWebHostEnvironment _environment;
        CORE.DTOs.APIs.TP_Services.APIsLists query = new CORE.DTOs.APIs.TP_Services.APIsLists();
        public WorkFlowController(IOptions<AppSettings> appSettings, IWebHostEnvironment environment, IMemoryCache memoryCache)
        {
            _environment = environment;
            _appSettings = appSettings.Value;
            _memoryCache = memoryCache;
            _memoryCache.TryGetValue(VehicleListCacheKey, out query);
        }
        public IActionResult Index(string err = null, string? Status = null)
        {
            ViewData["Error"] = err;
            ViewData["Filter"] = Status;
            MainSearchMC mainSearchMC = new MainSearchMC();
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadWorkFlowStage,
                Request = mainSearchMC
            };
            var WF = Helpers.ExcutePostAPI<List<WorkFlowStages>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");
            ViewData["WF"] = WF;
            return View();
        }

        public IActionResult UpdateWorkFlow(int Id)
        {
            if (Id > 0)
            {
                MainSearchMC mainSearchMC = new MainSearchMC()
                {
                    Id = Id
                };
                SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.LoadWorkFlowStage,
                    Request = mainSearchMC
                };
                var WF = Helpers.ExcutePostAPI<List<WorkFlowStages>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

                return View(WF.FirstOrDefault());
            }

            return View(new WorkFlowStages());
        }

        [HttpPost]
        public IActionResult UpdateWorkFlow(WorkFlowStages workFlowStages)
        {

            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateWorkFlowStage,
                Request = workFlowStages
            };
            var WF = Helpers.ExcutePostAPI<WorkFlowStages>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            return RedirectToAction("Index");
        }
        public IActionResult UpdateWorkFlowApproversV(int Id)
        {

            ViewData["WFId"] = Id;
            return View("UpdateWorkFlowApprovers",new WorkFlowApprovers());
        }


        [HttpPost]
        public IActionResult UpdateWorkFlowApprovers(WorkFlowApprovers workFlowApprovers)
        {

            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertWorkFlowApprovers,
                Request = workFlowApprovers
            };
            workFlowApprovers = Helpers.ExcutePostAPI<WorkFlowApprovers>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult SearchWorkFlow()
        {
            string? Status = HttpContext.Request.Form["Filter"];

            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Name = Status
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadWorkFlowStage,
                Request = mainSearchMC
            };
            var WF = Helpers.ExcutePostAPI<List<WorkFlowStages>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");
            ViewData["WF"] = WF;
            ViewData["Error"] = null;
            ViewData["Filter"] = Status;
            return View("Index");
        }

        public IActionResult LoadWorkFlowApprovers(int Id)
        {
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                WorkFlowId = Id
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadWorkFlowApprovers,
                Request = mainSearchMC
            };
            var approvers = Helpers.ExcutePostAPI<List<WorkFlowApprovers>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            return View(approvers);
        }
        public IActionResult DeleteApprover(int Id)
        {
            ViewData["Controller"] = "Workflow";
            ViewData["Action"] = "DeleteApproverPost";

            return View("_DeleteConfirmation", Id);
        }
        [HttpPost]
        public IActionResult DeleteApproverPost(int Id)
        {
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = Id
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.DeleteWorkFlowApprovers,
                Request = mainSearchMC
            };
            var Delegations = Helpers.ExcutePostAPI<bool>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            return RedirectToAction("Index");
        }
    }
}
