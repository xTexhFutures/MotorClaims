using CORE.DTOs.APIs.MotorClaim;
using CORE.DTOs.Authentications;
using CORE.DTOs.MotorClaim;
using CORE.DTOs.MotorClaim.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.WsTrust;
using MotorClaims.Models;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Engineering;
using System.Reflection;
using System.Security.Claims;
using X.PagedList;
using static MotorClaims.Models.Enums;

namespace MotorClaims.Controllers
{
    public class PaymentsController : Controller
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string LookupTable = "LookupTable";
        private readonly IMemoryCache _memoryCache;
        public static IWebHostEnvironment _environment;
        List<LookupTable> query = new List<LookupTable>();

        public PaymentsController(IOptions<AppSettings> appSettings, IWebHostEnvironment environment, IMemoryCache memoryCache)
        {
            _environment = environment;
            _appSettings = appSettings.Value;
            _memoryCache = memoryCache;
            _memoryCache.TryGetValue(LookupTable, out query);
        }

        public IActionResult Index(int page = 1, string? err = null)
        {
            if (!string.IsNullOrEmpty(err))
            {
                ViewData["Error"] = err;
            }
            List<ClaimMaster> claim = new List<ClaimMaster>();
            claim = HttpContext.Session.getSessionData<List<ClaimMaster>>("SearchResultPayment");
            IPagedList<ClaimMaster> claims = claim.ToPagedList(page, _appSettings.PageSize);
            ViewData["searchObj"] = new SearchObj();
            ViewData["query"] = query;
            ViewData["AllUsers"] = HttpContext.Session.getSessionData<List<Users>>("AllUsers");
            return View(claims);
        }

        [HttpPost]
        public IActionResult SearchPayments(SearchObj searchObj)
        {
            //if (string.IsNullOrEmpty(searchObj.nationalid) && !searchObj.RegisteredFrom.HasValue && !searchObj.RegisteredTo.HasValue && string.IsNullOrEmpty(searchObj.chassis) && string.IsNullOrEmpty(searchObj.claimno) && string.IsNullOrEmpty(searchObj.mobile) && string.IsNullOrEmpty(searchObj.policy))
            //{
            //    return RedirectToAction("Index", new { err = "Please fill at least one parameter" });
            //}
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                NationalID = searchObj.nationalid,
                chassis = searchObj.chassis,
                claimno = searchObj.claimno,
                mobile = searchObj.mobile,
                policy = searchObj.policy,
                RegisteredFrom = searchObj.RegisteredFrom,
                RegisteredTo = searchObj.RegisteredTo,
                ClaimStatus = (int)Enums.ClaimantStatus.Payment

            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
                Request = mainSearchMC
            };
            var claim = Helpers.ExcutePostAPI<List<ClaimMaster>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            IPagedList<ClaimMaster> Payments = claim.ToPagedList(1, _appSettings.PageSize);
            HttpContext.Session.SetSessionData("SearchResultPayment", claim);
            ViewData["searchObj"] = searchObj;
            ViewData["query"] = query;
            ViewData["AllUsers"] = HttpContext.Session.getSessionData<List<Users>>("AllUsers");
            return View("Index", Payments);
        }

        public IActionResult PaymentEntry(string obj, string? err = null)
        {
            if (!string.IsNullOrEmpty(err))
            {
                ViewData["Error"] = err;
            }
            ClaimSearchobj claimSearchobj = Helpers.Deserilize<ClaimSearchobj>(Helpers.Decryption(obj));
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = Convert.ToInt32(claimSearchobj.ClaimId)

            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
                Request = mainSearchMC
            };
            var claim = Helpers.ExcutePostAPI<List<ClaimMaster>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            ViewData["query"] = query;
            ViewData["DocumentsLink"] = _appSettings.DocumentsLink;
            ViewData["claimSearchobj"] = obj;
            ViewData["ClaimId"] = claimSearchobj.ClaimId;
            ViewData["ClaimantId"] = claimSearchobj.ClaimantId;
            Settlements settlements1 = new Settlements();
            settlements1 = HttpContext.Session.getSessionData<Settlements>("settlements");
            ViewData["settlements"] = settlements1 == null ? new Settlements() : settlements1;

            List<ReserveDetails> reserveDetails = new List<ReserveDetails>();
            foreach (var item in claim.FirstOrDefault().reserve)
            {
                foreach (var item1 in item.reserveDetails.Where(p=>p.IsClosed==false && p.IsSettled==false&&p.PremiaClaimId>0))
                {
                    reserveDetails.Add(item1);
                }
            }
            ViewData["reserveDetails"] = reserveDetails;
            return View(claim.FirstOrDefault());
        }


        [HttpPost]

        public IActionResult UpdatePayment(Settlements settlements, [FromForm(Name = "Invoice")] IFormFile file)
        {

            settlements.Creationdate = DateTime.Now;
            settlements.CreatedBy = HttpContext.Session.getSessionData<Users>("LoggedUser").Id;
            settlements.CreatedByName = HttpContext.Session.getSessionData<Users>("LoggedUser").UserName;
           
            string obj = HttpContext.Request.Form["obj"].ToString();
            HttpContext.Session.SetSessionData("settlements", settlements);
            Users users = new Users();
            users = HttpContext.Session.getSessionData<Users>("LoggedUser");
            string ReserveId =HttpContext.Request.Form["ReserveId"];

            if (Helpers.CheckPendingApprovals(settlements.ClaimantId, (int)Enums.WorkflowType.SettelmentAutherity, _appSettings))
            {
                return RedirectToAction("PaymentEntry", new { obj = obj, err = "There are pending Approval" });
            }
            if (string.IsNullOrEmpty(ReserveId))
            {
                return RedirectToAction("PaymentEntry", new { obj = obj, err = "At least select one reserve" });
            }

            SetupClaimsRequestcs mainSearch = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
                Request = new MainSearchMC()
                {
                    Id = (int)settlements.ClaimId
                }
            };
            var claims = Helpers.ExcutePostAPI<List<ClaimMaster>>(mainSearch, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            List<ReserveDetails> reserveDetails = new List<ReserveDetails>();
            foreach (var item in claims.FirstOrDefault().reserve)
            {
                foreach (var item1 in item.reserveDetails.Where(p => p.IsClosed == false && p.IsSettled == false))
                {
                    reserveDetails.Add(item1);
                }
            }
            decimal TotalSettlement = 0;
            foreach (var reserve in ReserveId.Split(','))
            {
                TotalSettlement += reserveDetails.Where(p => p.Id == Convert.ToInt32(reserve)).FirstOrDefault().Amount;
            }
            settlements.Status = users.ClaimApprovalAuthority > TotalSettlement ? (int)Enums.Status.InProgress : (int)Enums.Status.WaitingApproval;


            if (file != null && file.Length > 0)
            {
                int _min = 1000;
                int _max = 9999;
                string error = string.Empty;
                Random _rdm = new Random();
                int otpPick = _rdm.Next(_min, _max);

                string pathMDF = _appSettings.ClaimSubmissionPath;
                string fieNameWithExt = otpPick.ToString() + "_" + settlements.ClaimantId.ToString() + "_" + Path.GetFileName(file.FileName);
                string directory = Path.Combine(pathMDF, claims.FirstOrDefault().claims.ClaimNo, claims.FirstOrDefault().claimants.Serial.ToString());
                string filePath = Path.Combine(directory, fieNameWithExt);
                settlements.InvoicePath = _appSettings.ClaimSubmissionURL + "//" + claims.FirstOrDefault().claims.ClaimNo + "//" + claims.FirstOrDefault().claimants.Serial.ToString() + "//" + fieNameWithExt;
                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
            }
            else
            {
                return RedirectToAction("PaymentEntry", new { obj = obj, err = "Upload Invoice Copy !!" });
            }
            ClaimTransactions claimTransactions = new ClaimTransactions()
            {
                ClaimantID = settlements.ClaimantId,
                ClaimId = settlements.ClaimId,
                CollectionType = string.Empty,
                Collector = 1,
                Commission = 0,
                Fees = 0,
                isActive = users.ClaimApprovalAuthority > TotalSettlement ? true : false,
                ParentTransactions = null,
                Payment = string.Empty,
                TransactionAmount = TotalSettlement,
                TransactionDate = DateTime.Now,
                TransactionType = (int)Enums.ClaimTransactionTypes.Payment,
                CreatedBy = HttpContext.Session.getSessionData<Users>("LoggedUser").UserName,
                Note = null
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimTransaction,
                Request = claimTransactions
            };
            claimTransactions = Helpers.ExcutePostAPI<ClaimTransactions>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            if (users.ClaimApprovalAuthority < TotalSettlement)
            {
                Helpers.PublishWorkflow(claimTransactions.TransactionAmount, Enums.WorkflowType.SettelmentAutherity, claimTransactions.Id, claimTransactions.ClaimId, claimTransactions.ClaimantID, claimTransactions.CreatedBy, HttpContext.Session.getSessionData<List<Users>>("AllUsers"), _appSettings);
                settlements.Status = (int)Enums.Status.WaitingApproval;
            }
            else
            {
                foreach (var reserve in ReserveId.Split(','))
                {
                    string error = string.Empty; bool Result = true;
                    PremiaIntegration.CreateClaimSettlement(claims.FirstOrDefault(), settlements, reserveDetails.Where(p=>p.Id==Convert.ToInt32(reserve)).FirstOrDefault(), _appSettings, out Result, out error);
                }

                Claimants claimants = new Claimants();
                claimants = claims.FirstOrDefault().claimants;
                claimants.ClaimantStatus = (int)Enums.ClaimantStatus.Operation;

                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimants,
                    Request = claimants
                };
                claimants = Helpers.ExcutePostAPI<Claimants>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            }

            settlements.TransactionId = claimTransactions.Id;
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertSettlement,
                Request = settlements
            };
            settlements = Helpers.ExcutePostAPI<Settlements>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            return RedirectToAction("Index","Home", new { obj = obj });
        }

        [HttpPost]
        public IActionResult UpdatePaymentFinance(Settlements settlements)
        {
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = (int)settlements.Id
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadSettlements,
                Request = mainSearchMC
            };
            var claims = Helpers.ExcutePostAPI<List<Settlements>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            Settlements settlements1 = new Settlements();

            settlements1 = claims.FirstOrDefault();
            settlements1.TransferReference = settlements.TransferReference;
            settlements1.Status = (int)Enums.Status.Paid;
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertSettlement,
                Request = settlements1
            };
            settlements1 = Helpers.ExcutePostAPI<Settlements>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");



             mainSearchMC = new MainSearchMC()
            {
                Id =settlements1.ClaimantId
            };
             setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimants,
                Request = mainSearchMC
            };
            var claimants = Helpers.ExcutePostAPI<List<Claimants>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            Claimants claimant = new Claimants();
            claimant = claimants.FirstOrDefault();
            claimant.ClaimantStatus = (int)Enums.ClaimantStatus.Operation;
            claimant.StatusReason = "Payment Transferred";

            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimants,
                Request = claimant
            };
            claimant = Helpers.ExcutePostAPI<Claimants>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            Helpers.RegisterHistory(_appSettings, settlements.ClaimId, "Confirm Payment Transfer by " + HttpContext.Session.getSessionData<Users>("LoggedUser").UserName, HttpContext.Session.getSessionData<Users>("LoggedUser").UserName, settlements.ClaimantId);
            HttpContext.Session.SetSessionData("SearchResultPayment", null);
            return RedirectToAction("Index", "Home");
        }
    }
}
