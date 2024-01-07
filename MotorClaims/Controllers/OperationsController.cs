using CORE.DTOs.APIs.MotorClaim;
using CORE.DTOs.APIs.TP_Services;
using CORE.DTOs.Authentications;
using CORE.DTOs.MotorClaim;
using CORE.DTOs.MotorClaim.Claims;
using CORE.DTOs.Setups;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MotorClaims.Models;
using System.Security.Claims;
using X.PagedList;

namespace MotorClaims.Controllers
{
    public class OperationsController : Controller
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string VehicleListCacheKey = "LookupTable";
        private readonly IMemoryCache _memoryCache;
        public static IWebHostEnvironment _environment;
        List<LookupTable> query = new List<LookupTable>();

        public OperationsController(IOptions<AppSettings> appSettings, IWebHostEnvironment environment, IMemoryCache memoryCache)
        {
            _environment = environment;
            _appSettings = appSettings.Value;
            _memoryCache = memoryCache;
            _memoryCache.TryGetValue(VehicleListCacheKey, out query);
        }
        public IActionResult Index(int page=1,string? err=null)
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
        public IActionResult SearchOperations(SearchObj searchObj)
        {
            List<ClaimMaster> claim = new List<ClaimMaster>();
            IPagedList<ClaimMaster> Operations= claim.ToPagedList(1, _appSettings.PageSize);
            ViewData["searchObj"] = searchObj;
            ViewData["AllUsers"] = HttpContext.Session.getSessionData<List<Users>>("AllUsers");
            if (string.IsNullOrEmpty(searchObj.nationalid) && !searchObj.RegisteredFrom.HasValue && !searchObj.RegisteredTo.HasValue  && string.IsNullOrEmpty(searchObj.chassis) && string.IsNullOrEmpty(searchObj.claimno) && string.IsNullOrEmpty(searchObj.mobile) && string.IsNullOrEmpty(searchObj.policy))
            {
                return RedirectToAction("Index",new {err= "Please fill at least one parameter" } );
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

        public IActionResult OperationEntry(string obj)
        {
            ClaimSearchobj claimSearchobj = Helpers.Deserilize<ClaimSearchobj>(Helpers.Decryption(obj));
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = Convert.ToInt32(claimSearchobj.ClaimId),
                ClaimantId= claimSearchobj.ClaimantId

            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
                Request = mainSearchMC
            };
            var claim = Helpers.ExcutePostAPI<List<ClaimMaster>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            ViewData["query"] = query;
            ViewData["obj"] = obj;
            ViewData["DocumentsLink"] = _appSettings.DocumentsLink;
            
            return View(claim.FirstOrDefault());
        }
        public IActionResult OperationAssign(int ClaimId,int ClaimantId)
        {
            ViewData["DivName"] = "OperationAssign";
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                ClaimantId = ClaimantId

            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
                Request = mainSearchMC
            };
            var claim = Helpers.ExcutePostAPI<List<ClaimMaster>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            ViewData["AllUsers"] = HttpContext.Session.getSessionData<List<Users>>("AllUsers");
            return View("_SurveyorAssign", claim.FirstOrDefault());
        }
        public IActionResult UpdateReserve(int ClaimId)
        {
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = ClaimId
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaim,
                Request = mainSearchMC
            };
            var claim = Helpers.ExcutePostAPI<List<Claims>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            ViewData["ClaimMaster"] = HttpContext.Session.getSessionData<List<ClaimMaster>>("ClaimMaster");
            return View("_UpdateReserve", claim.FirstOrDefault());
        }  
        
        public IActionResult OperationReOpen(int ClaimId)
        {
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = ClaimId
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaim,
                Request = mainSearchMC
            };
            var claim = Helpers.ExcutePostAPI<List<Claims>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            mainSearchMC = new MainSearchMC()
            {
                ClaimId = ClaimId
            };
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimants,
                Request = mainSearchMC
            };
            var claimants = Helpers.ExcutePostAPI<List<Claimants>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            ViewData["claimants"] = claimants;
            ViewData["ClaimMaster"] = HttpContext.Session.getSessionData<List<ClaimMaster>>("ClaimMaster");
            return View("_OperationReOpen", claim.FirstOrDefault());
        }
        public IActionResult UpdateRecovery(int ClaimId,int? ClaimantId=null)
        {
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = ClaimId
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaim,
                Request = mainSearchMC
            };
            var claim = Helpers.ExcutePostAPI<List<Claims>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            ViewData["claimantId"] = ClaimantId;
            ViewData["ClaimMaster"] = HttpContext.Session.getSessionData<List<ClaimMaster>>("ClaimMaster");
            return View("_UpdateRecovery", claim.FirstOrDefault());
        }

        [HttpPost]
        public IActionResult ReOpenClaim()
        {
            int ClaimId = Convert.ToInt32(HttpContext.Request.Form["ClaimId"]);
            int? ClaimantID = HttpContext.Request.Form["ClaimantID"]==""?null: Convert.ToInt32(HttpContext.Request.Form["ClaimantID"]);
            string Reason = HttpContext.Request.Form["Note"];
            string Language = HttpContext.Session.getSessionData<string>("Lang").ToUpper();
            

                MainSearchMC mainSearchMC = new MainSearchMC()
                {
                    Id = ClaimId,
                    ClaimantId = ClaimantID,
                };
                SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaim,
                    Request = mainSearchMC
                };
              var  claims = Helpers.ExcutePostAPI<List<Claims>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            Claims claim = new Claims();
            claim = claims.FirstOrDefault();
            //claim.ClaimStatus = (int)Enums.ClaimStatus.ReOpen;

             setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaim,
                Request = claim
            };
            claim = Helpers.ExcutePostAPI<Claims>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            Helpers.RegisterHistory(_appSettings, claim.Id, Reason, HttpContext.Session.getSessionData<Users>("LoggedUser").UserName,1);
            Helpers.SendNotificationMail(Language, _appSettings, _appSettings.OperationEmail, claim.ClaimNo);

            return View("_OperationReOpen", claim);
        }      
        
        
        [HttpPost]
        public IActionResult UpdateAssign()
        {
            int ClaimId = Convert.ToInt32(HttpContext.Request.Form["ClaimId"]);
            int ClaimantId = Convert.ToInt32(HttpContext.Request.Form["ClaimantId"]);
            int Validation = Convert.ToInt32(HttpContext.Request.Form["Validation"]);
            string Language = HttpContext.Session.getSessionData<string>("Lang").ToUpper();
            List<Users> Allusers = HttpContext.Session.getSessionData<List<Users>>("AllUsers");
            int AssignToId = 0, TeamAssignTo = 0;
            Claimants claimants = new Claimants();
            if (Validation == 2)
            {
                AssignToId = Convert.ToInt32(HttpContext.Request.Form["AssignToId"]);
                MainSearchMC mainSearchMC = new MainSearchMC()
                {
                    ClaimId = ClaimId,
                    UserId = AssignToId,
                    ClaimantId=ClaimantId,
                };
                SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.UpdateAssign,
                    Request = mainSearchMC
                };
                claimants = Helpers.ExcutePostAPI<Claimants>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

               //string EmailBody = Language == "AR-JO" ? System.IO.File.ReadAllText(Path.Combine(_appSettings.EmailsFolder, "Notification-AR.html")) : System.IO.File.ReadAllText(Path.Combine(_appSettings.EmailsFolder, "Notification-EN.html"));

               // EmailInput emailInput = new EmailInput()
               // {
               //     Body =EmailBody,
               //     Subject = Language == "AR-JO"? "تنبيه مطالبة - "+ claims.ClaimNo: "Claim Notification - " + claims.ClaimNo,
               //     ToEmail = Allusers.Where(p => p.Id == AssignToId).FirstOrDefault().Email
               // };
               // var PostingResult = Helpers.ExcutePostAPI<CORE.DTOs.APIs.Unified_Response.Results>(emailInput, _appSettings.APIHubPrefix + "api/ExternalAPIs/SendEmail");
            }
            else
            {
                TeamAssignTo = Convert.ToInt32(HttpContext.Request.Form["teamAssign"]);
                AutoAssignObj autoAssignObj = new AutoAssignObj()
                {
                    ClaimantId = ClaimantId,
                    RoleId = TeamAssignTo
                };
                claimants = Helpers.ExcutePostAPI<Claimants>(autoAssignObj, _appSettings.APIHubPrefix + "api/MotorClaim/AutoAssign");

                //string EmailBody = Language == "AR-JO" ? System.IO.File.ReadAllText(Path.Combine(_appSettings.EmailsFolder, "Notification-AR.html")) : System.IO.File.ReadAllText(Path.Combine(_appSettings.EmailsFolder, "Notification-EN.html"));

                //EmailInput emailInput = new EmailInput()
                //{
                //    Body = EmailBody,
                //    Subject = Language == "AR-JO" ? "تنبيه مطالبة - " + claims.ClaimNo : "Claim Notification - " + claims.ClaimNo,
                //    ToEmail = _appSettings.OperationEmail
                //};
                //var PostingResult = Helpers.ExcutePostAPI<CORE.DTOs.APIs.Unified_Response.Results>(emailInput, _appSettings.APIHubPrefix + "api/ExternalAPIs/SendEmail");
            }

            ViewData["DivName"] = "OperationAssign";
            MainSearchMC mainSearchMC1 = new MainSearchMC()
            {
                ClaimantId = ClaimantId

            };
            SetupClaimsRequestcs setupClaimsRequestcs1 = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
                Request = mainSearchMC1
            };
            var claim = Helpers.ExcutePostAPI<List<ClaimMaster>>(setupClaimsRequestcs1, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            return View("_SurveyorAssign", claim.FirstOrDefault());
        }


        public IActionResult eClaims(int page=1)
        {
            eClaimsObj eClaims = new eClaimsObj();
            List<eClaims> eClaims1 = new List<eClaims>();
            ViewData["eClaims"] = eClaims;
            IPagedList<eClaims> lclaims = eClaims1.ToPagedList(page, _appSettings.PageSize);
            return View(lclaims);
        }


        [HttpPost]
        public IActionResult SearcheClaims(eClaimsObj obj,int page=1)
        {
            var claim = Helpers.ExcutePostAPI<List<eClaims>>(obj, _appSettings.APIHubPrefix + "api/MotorClaim/LoadeClaims");
            ViewData["eClaims"] = obj;
            IPagedList<eClaims> lclaims = claim.ToPagedList(page, _appSettings.PageSize);
            return View("eClaims", lclaims);
        }
        [HttpPost]
        public ReserveBalance GetReserveBalance(int Id)
        {

            ReserveBalance reserveBalance = new ReserveBalance();
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                ClaimantId = Id,
                ClaimId=0
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.ReserveBalance,
                Request = mainSearchMC
            };
            reserveBalance = Helpers.ExcutePostAPI<ReserveBalance>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            return reserveBalance;
        }


        [HttpPost]
        public IActionResult UpdateClaimStatus()
        {
            int ClaimId = Convert.ToInt32(HttpContext.Request.Form["Id"]);
            int ClaimantId = Convert.ToInt32(HttpContext.Request.Form["ClaimantId"]);
            int ClaimStatus = Convert.ToInt32(HttpContext.Request.Form["ClaimStatus"]);
            string obj = HttpContext.Request.Form["obj"];
            string RejectionReason = HttpContext.Request.Form["RejectionReason"];

            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = ClaimantId
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimants,
                Request = mainSearchMC
            };
           var claimants = Helpers.ExcutePostAPI<List<Claimants>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            if (claimants.Count>0)
            {
                claimants.FirstOrDefault().ClaimantStatus = ClaimStatus;
                claimants.FirstOrDefault().StatusReason = RejectionReason;

                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimants,
                    Request = claimants.FirstOrDefault()
                };
                var claimant = Helpers.ExcutePostAPI<Claimants>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
                Helpers.RegisterHistory(_appSettings, claimant.ClaimId, "Update Claim status to " + ((Enums.ClaimantStatus)ClaimStatus).ToString(), HttpContext.Session.getSessionData<Users>("LoggedUser").UserName, claimant.Id);
            }
            return RedirectToAction("OperationEntry", new { obj = obj });
        }
    }
}
