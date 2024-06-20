using CORE.DTOs.APIs.MotorClaim;
using CORE.DTOs.Authentications;
using CORE.DTOs.MotorClaim;
using CORE.DTOs.MotorClaim.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MotorClaims.Models;
using Newtonsoft.Json;
using System.Security.Claims;
using X.PagedList;

namespace MotorClaims.Controllers
{
    public class ApprovalsController : Controller
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string VehicleListCacheKey = "LookupTable";
        private readonly IMemoryCache _memoryCache;
        public static IWebHostEnvironment _environment;
        List<LookupTable> query = new List<LookupTable>();

        public ApprovalsController(IOptions<AppSettings> appSettings, IWebHostEnvironment environment, IMemoryCache memoryCache)
        {
            _environment = environment;
            _appSettings = appSettings.Value;
            _memoryCache = memoryCache;
            _memoryCache.TryGetValue(VehicleListCacheKey, out query);
        }
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
        public IActionResult SearchApprovalss(SearchObj searchObj)
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
                ClaimStatus = (int)Enums.ClaimantStatus.Operation

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

        public IActionResult ApprovalEntry(string obj)
        {     
            ClaimSearchobj claimSearchobj = Helpers.Deserilize<ClaimSearchobj>(Helpers.Decryption(obj));
            ViewData["query"] = query;
            ViewData["obj"] = obj;
            ViewData["Id"] = claimSearchobj.Id;
            ApprovalStatus approvalStatus = new ApprovalStatus();
 
            ViewData["approvalStatus"] = approvalStatus;
            ViewData["DocumentsLink"] = _appSettings.DocumentsLink;

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

            mainSearchMC = new MainSearchMC()
            {
                Id = claimSearchobj.Id

            };
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadWorkflowTransactions,
                Request = mainSearchMC
            };
            var workflowTransaction = Helpers.ExcutePostAPI<List<WorkflowTransaction>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            if (workflowTransaction.FirstOrDefault().ClaimTransactionId.HasValue && workflowTransaction.FirstOrDefault().ClaimTransactionId.Value>0)
            {
                mainSearchMC = new MainSearchMC()
                {
                    Id = workflowTransaction.FirstOrDefault().ClaimTransactionId
                };
                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimTransactions,
                    Request = mainSearchMC
                };
                var Reserve = Helpers.ExcutePostAPI<List<ClaimTransactions>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
                ViewData["Reserve"] = Reserve.FirstOrDefault();

            }
            return View(claim.FirstOrDefault());
        }

        [HttpPost]
        public IActionResult UpdateApprovalStatus()
        {
            int Id= Convert.ToInt32(HttpContext.Request.Form["Id"]);
            int ApprovalStatus = Convert.ToInt32(HttpContext.Request.Form["ApprovalStatus"]);
            string RejectionReason = HttpContext.Request.Form["RejectionReason"];
            string Obj = HttpContext.Request.Form["obj"];
            ClaimSearchobj claimSearchobj = Helpers.Deserilize<ClaimSearchobj>(Helpers.Decryption(Obj));
            claimSearchobj.ApprovalId = Id;
            ApprovalStatus approvalStatus = new ApprovalStatus()
            {
                Id = Id,
                Reason = RejectionReason,
                Status = ApprovalStatus==1?true:false
            };

            List<WorkflowTransactionApprovers> workflowTransactionApprovers = new List<WorkflowTransactionApprovers>();
            WorkflowTransactionApprovers workflowTransactionApprover = new WorkflowTransactionApprovers();
            WorkflowTransaction workflowTransaction1 = new WorkflowTransaction();
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                TransactionId = claimSearchobj.ApprovalId

            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadWorkflowTransactionApprovers,
                Request = mainSearchMC
            };
            workflowTransactionApprovers = Helpers.ExcutePostAPI<List<WorkflowTransactionApprovers>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            workflowTransactionApprover = workflowTransactionApprovers.Where(p => p.UserId == HttpContext.Session.getSessionData<Users>("LoggedUser").Id).FirstOrDefault();
            workflowTransactionApprover.Note = RejectionReason;
            workflowTransactionApprover.Action = ApprovalStatus == 1 ? true : false;

             setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertWorkflowTransactionApprovers,
                Request = workflowTransactionApprover
             };
            workflowTransactionApprover = Helpers.ExcutePostAPI<WorkflowTransactionApprovers>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            mainSearchMC = new MainSearchMC()
            {
                Id = claimSearchobj.ApprovalId

            };
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadWorkflowTransactions,
                Request = mainSearchMC
            };
            var workflowTransaction = Helpers.ExcutePostAPI<List<WorkflowTransaction>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            if (ApprovalStatus == 1)
            {

                if (workflowTransaction.FirstOrDefault().ClaimTransactionId.HasValue && workflowTransaction.FirstOrDefault().ClaimTransactionId.Value>0)
                {
                    ClaimTransactions claimTransactions = new ClaimTransactions();
                    Reserve reserve = new Reserve();
                    mainSearchMC = new MainSearchMC()
                    {
                        Id = workflowTransaction.FirstOrDefault().ClaimTransactionId
                    };
                    setupClaimsRequestcs = new SetupClaimsRequestcs()
                    {
                        TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimTransactions,
                        Request = mainSearchMC
                    };
                    var Reserve = Helpers.ExcutePostAPI<List<ClaimTransactions>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

                    claimTransactions = Reserve.FirstOrDefault();
                    claimTransactions.isActive = true;

                    setupClaimsRequestcs = new SetupClaimsRequestcs()
                    {
                        TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimTransaction,
                        Request = claimTransactions
                    };
                    var Reserve2 = Helpers.ExcutePostAPI<ClaimTransactions>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

                    mainSearchMC = new MainSearchMC()
                    {
                        TransactionId = workflowTransaction.FirstOrDefault().ClaimTransactionId
                    };
                    setupClaimsRequestcs = new SetupClaimsRequestcs()
                    {
                        TransactionType = CORE.Extensions.ClaimTransactionType.LoadReserve,
                        Request = mainSearchMC
                    };
                    var Reserve3 = Helpers.ExcutePostAPI<List<Reserve>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

                    reserve = Reserve3.FirstOrDefault();
                    reserve.Status = true;

                    setupClaimsRequestcs = new SetupClaimsRequestcs()
                    {
                        TransactionType = CORE.Extensions.ClaimTransactionType.InsertReserve,
                        Request = reserve
                    };
                    reserve = Helpers.ExcutePostAPI<Reserve>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

                }

            }
            workflowTransaction1=workflowTransaction.FirstOrDefault();
            workflowTransaction1.Status = ApprovalStatus;
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertWorkflowTransaction,
                Request = workflowTransaction1
            };
            workflowTransaction1 = Helpers.ExcutePostAPI<WorkflowTransaction>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            Helpers.RegisterHistory(_appSettings, claimSearchobj.ClaimId.Value, ApprovalStatus == 1? "Approve Workflow": "Reject Workflow", HttpContext.Session.getSessionData<Users>("LoggedUser").UserName, claimSearchobj.ClaimantId.Value);
            return RedirectToAction("Index", "Home");
        }
    }
}
