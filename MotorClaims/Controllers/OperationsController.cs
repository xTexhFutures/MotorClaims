using CORE.DTOs.APIs.MotorClaim;
using CORE.DTOs.APIs.TP_Services;
using CORE.DTOs.APIs.TPServices;
using CORE.DTOs.Authentications;
using CORE.DTOs.MotorClaim;
using CORE.DTOs.MotorClaim.Claims;
using CORE.DTOs.MotorClaim.Integrations.APIs;
using CORE.DTOs.Setups;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MotorClaims.Models;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using X.PagedList;
using static MotorClaims.Models.Enums;

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
        [CustomAuthorize(Roles = "Admin,Opertion")]
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
        public IActionResult SearchOperations(SearchObj searchObj)
        {
            List<ClaimMaster> claim = new List<ClaimMaster>();
            IPagedList<ClaimMaster> Operations = claim.ToPagedList(1, _appSettings.PageSize);
            ViewData["searchObj"] = searchObj;
            ViewData["AllUsers"] = HttpContext.Session.getSessionData<List<Users>>("AllUsers");
            if (string.IsNullOrEmpty(searchObj.nationalid) && !searchObj.RegisteredFrom.HasValue && !searchObj.RegisteredTo.HasValue && string.IsNullOrEmpty(searchObj.chassis) && string.IsNullOrEmpty(searchObj.claimno) && string.IsNullOrEmpty(searchObj.mobile) && string.IsNullOrEmpty(searchObj.policy) && string.IsNullOrEmpty(HttpContext.Request.Form["RegisteredFrom"]))
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
                RegisteredFrom = searchObj.RegisteredFrom.HasValue ? Helpers.ConvertDate(searchObj.RegisteredFrom.Value.ToString("dd/MM/yyyy"))  : (!string.IsNullOrEmpty(HttpContext.Request.Form["RegisteredFrom"]) ? Helpers.ConvertDate(HttpContext.Request.Form["RegisteredFrom"]) : null),
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
        public IActionResult OperationEntry(string obj, string err = null, bool? Result = false)
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
            ViewData["DocumentsLink"] = _appSettings.DocumentsLink;


            mainSearchMC = new MainSearchMC()
            {
                ClaimantId = claimSearchobj.ClaimantId
            };
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadSMSLogs,
                Request = mainSearchMC
            };
            var sms = Helpers.ExcutePostAPI<List<SMSLog>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");


            ViewData["SMSLog"] = sms;
            ViewData["workshops"] = claim.FirstOrDefault().vehiclesInfo.RepairCondition == (int)Enums.RepairConditions.Workshop ? HttpContext.Session.getSessionData<List<Users>>("workshops") : HttpContext.Session.getSessionData<List<Users>>("Agencies");
            ViewData["MissingDocuments"] = Helpers.GetMissingDocuments(claim.FirstOrDefault().claimants, _appSettings);
            if (!string.IsNullOrEmpty(err) && Result.HasValue && !Result.Value)
            {
                ViewData["Error"] = err;
            }
            if (!string.IsNullOrEmpty(err) && Result.HasValue && Result.Value)
            {
                ViewData["Error2"] = err;
            }
            string error = string.Empty; bool Result1 = false;
            if (claim!=null && claim.FirstOrDefault().claims.PremiaClaimId.HasValue && claim.FirstOrDefault().claims.PremiaClaimId.Value>0 && !claim.FirstOrDefault().claimants.PremiaClaimId.HasValue)
            {
                PremiaIntegration.CreateTPClaim(claim.FirstOrDefault(), _appSettings, out Result1, out error);
            }
         
            return View(claim.FirstOrDefault());
        }
        public IActionResult OperationAssign(int ClaimId, int ClaimantId)
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



        public IActionResult UpdateReserve(int ClaimId, int ClaimantId, string? err = null)
        {
            ViewData["Error"] = string.Empty;
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = ClaimId,
                ClaimantId = ClaimantId
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
                Request = mainSearchMC
            };
            var claim = Helpers.ExcutePostAPI<List<ClaimMaster>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            if (!string.IsNullOrEmpty(err))
            {
                ViewData["Error"] = err;
            }
            HttpContext.Session.SetSessionData("ClaimMasterReserve", claim.FirstOrDefault());
            ViewData["ReserveCodes"] = query.Where(p => p.MajorCode == (int)Lookups.ReserveCodes).ToList();

            string error = string.Empty; bool Result1 = false;
            if (claim != null && claim.FirstOrDefault().claims.PremiaClaimId.HasValue && claim.FirstOrDefault().claims.PremiaClaimId.Value > 0 && !claim.FirstOrDefault().claimants.PremiaClaimId.HasValue)
            {
                PremiaIntegration.CreateTPClaim(claim.FirstOrDefault(), _appSettings, out Result1, out error);
            }
            return View("_UpdateReserve", claim.FirstOrDefault());
        }

        public IActionResult OperationReOpen(int ClaimId, int ClaimantId)
        {
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = ClaimId,
                ClaimantId = ClaimantId
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
                Request = mainSearchMC
            };
            var claim = Helpers.ExcutePostAPI<List<ClaimMaster>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            return View("_OperationReOpen", claim.FirstOrDefault());
        }
        public IActionResult UpdateRecovery(int ClaimId, int? ClaimantId = null)
        {
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = ClaimId,
                ClaimantId = ClaimantId
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
                Request = mainSearchMC
            };
            var claim = Helpers.ExcutePostAPI<List<ClaimMaster>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            return View("_UpdateRecovery", claim.FirstOrDefault());
        }

        [HttpPost]
        public IActionResult ReOpenClaim()
        {
            int ClaimId = Convert.ToInt32(HttpContext.Request.Form["ClaimId"]);
            int? ClaimantID = HttpContext.Request.Form["ClaimantID"] == "" ? null : Convert.ToInt32(HttpContext.Request.Form["ClaimantID"]);
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
            var claims = Helpers.ExcutePostAPI<List<Claims>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            Claims claim = new Claims();
            claim = claims.FirstOrDefault();
            //claim.ClaimStatus = (int)Enums.ClaimStatus.ReOpen;

            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaim,
                Request = claim
            };
            claim = Helpers.ExcutePostAPI<Claims>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            Helpers.RegisterHistory(_appSettings, claim.Id, Reason, HttpContext.Session.getSessionData<Users>("LoggedUser").UserName, 1);
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
                    ClaimantId = ClaimantId,
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
            string UserName = Allusers.Where(x => x.Id == claimants.AssignTo).FirstOrDefault().UserName;
            Helpers.RegisterHistory(_appSettings, claimants.ClaimId, "Reassign To " + UserName + " By " + HttpContext.Session.getSessionData<Users>("LoggedUser").UserName, HttpContext.Session.getSessionData<Users>("LoggedUser").UserName, claimants.Id);
            return View("_SurveyorAssign", claim.FirstOrDefault());
        }


        public IActionResult eClaims(int page = 1)
        {
            eClaimsObj eClaims = new eClaimsObj();
            List<eClaims> eClaims1 = new List<eClaims>();
            ViewData["eClaims"] = eClaims;
            ViewData["eClaimsURL"] = _appSettings.eClaimsURL;
            IPagedList<eClaims> lclaims = eClaims1.ToPagedList(page, _appSettings.PageSize);
            return View(lclaims);
        }


        [HttpPost]
        public IActionResult SearcheClaims(eClaimsObj obj, int page = 1)
        {
            if (!string.IsNullOrEmpty(obj.RegisteredFromS))
            {
                obj.RegisteredFrom = Helpers.ConvertDate(obj.RegisteredFromS);
            }
            if (!string.IsNullOrEmpty(obj.RegisteredToS))
            {
                obj.RegisteredTo = Helpers.ConvertDate(obj.RegisteredToS);
            }
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
                ClaimId = 0
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
        public IActionResult UpdateClaimStatus(IFormFile fromFiles)
        {
            bool Result = true; string error = string.Empty;
            int ClaimId = Convert.ToInt32(HttpContext.Request.Form["Id"]);
            int ClaimantId = Convert.ToInt32(HttpContext.Request.Form["ClaimantId"]);
            int ClaimStatus = Convert.ToInt32(HttpContext.Request.Form["ClaimStatus"]);
            int ClaimantStatus = Convert.ToInt32(HttpContext.Request.Form["ClaimantStatus"]);
            string obj = HttpContext.Request.Form["obj"];
            string ClaimNo = HttpContext.Request.Form["ClaimNo"];
            string RejectionReason = HttpContext.Request.Form["RejectionReason"];
            string CloseReason = HttpContext.Request.Form["CloseReason"];
            Claimants claimant = new Claimants();
            Attachments attachment = new Attachments();


            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = (int)ClaimId,
                ClaimantId= ClaimantId
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
                Request = mainSearchMC
            };
            var claims = Helpers.ExcutePostAPI<List<ClaimMaster>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");


            if (claims.FirstOrDefault().claimants!=null)
            {
                claimant = claims.FirstOrDefault().claimants;
            }
            else
            {
                return RedirectToAction("OperationEntry", new { obj = obj });
            }

            if (ClaimStatus == (int)Enums.ClaimantStatus.Workshop)
            {
                int TransactionType = Convert.ToInt32(HttpContext.Request.Form["TransactionType"]);
                int Workshops = Convert.ToInt32(HttpContext.Request.Form["Workshops"]);
                if (fromFiles != null)
                {
                    string fieNameWithExt = Path.GetFileName(fromFiles.FileName);
                    if (fromFiles.Length > 0)
                    {
                        NetworkCredential networkCredential = new NetworkCredential("Administrator", "P@ssw0rd");
                        CredentialCache credentialCache = new CredentialCache();
                        credentialCache.Add(new Uri(@"\\networkshare\"), "Basic", networkCredential);

                        string filePath = Path.Combine(_appSettings.ClaimSubmissionPath, ClaimNo, claimant.Serial.ToString());
                        bool folderExists = Directory.Exists(filePath);
                        if (!folderExists)
                            Directory.CreateDirectory(filePath);

                        using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            fromFiles.CopyToAsync(fileStream);
                        }
                        attachment = new Attachments()
                        {
                            ClaimantId = ClaimantId,
                            ClaimId = ClaimId,
                            CreationDate = DateTime.Now,
                            DocumentSetupId = TransactionType == 1 ? 1014 : 1016,
                            ModuleId = (int)Enums.DocumnetType.Operations,
                            FileName = Path.GetFileName(fromFiles.FileName),
                            ContentType = fromFiles.ContentType,
                            CreatedBy = HttpContext.Session.getSessionData<Users>("LoggedUser").UserName,
                            IsDeleted = false
                        };
                        setupClaimsRequestcs = new SetupClaimsRequestcs()
                        {
                            TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateAttachment,
                            Request = attachment
                        };
                        attachment = Helpers.ExcutePostAPI<Attachments>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");
                    }
                }
                claimant.AssignTo = Workshops;

            }
            else if (ClaimantStatus == (int)Enums.ClaimantStatus.Closed)
            {
                if (Helpers.CheckPendingApprovals(ClaimantId, 0, _appSettings))
                {
                    return RedirectToAction("OperationEntry", new { obj = obj, err = "There are pending Approvals !!", Result = false });
                }

                mainSearchMC = new MainSearchMC()
                {
                    ClaimId = Convert.ToInt32(ClaimId),
                    ClaimantId = ClaimantId
                };
                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.ReserveBalance,
                    Request = mainSearchMC
                };

                var ReserveBalance = Helpers.ExcutePostAPI<ReserveBalance>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

                if (ReserveBalance != null && ReserveBalance.ClaimantLevel != 0)
                {
                    return RedirectToAction("OperationEntry", new { obj = obj, err = "There are Booked reserve please make it Zero first !!", Result = false });
                }

                claimant.ClaimantStatus = ClaimantStatus;
                claimant.StatusReason = CloseReason;

                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimants,
                    Request = claimant
                };
                claimant = Helpers.ExcutePostAPI<Claimants>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
                Helpers.RegisterHistory(_appSettings, claimant.ClaimId, "Update Claim status to " + ((Enums.ClaimantStatus)ClaimantStatus).ToString(), HttpContext.Session.getSessionData<Users>("LoggedUser").UserName, claimant.Id);

            }
            else if (ClaimantStatus == (int)Enums.ClaimantStatus.Payment)
            {
                claimant.ClaimantStatus = ClaimantStatus;
                claimant.StatusReason = CloseReason;

                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimants,
                    Request = claimant
                };
                claimant = Helpers.ExcutePostAPI<Claimants>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
                Helpers.RegisterHistory(_appSettings, claimant.ClaimId, "Update Claim status to " + ((Enums.ClaimantStatus)ClaimantStatus).ToString(), HttpContext.Session.getSessionData<Users>("LoggedUser").UserName, claimant.Id);

            }
            else if (ClaimStatus == (int)Enums.ClaimantStatus.Rejected)
            {
                claimant.ClaimantStatus = ClaimStatus;
                claimant.StatusReason = RejectionReason;

                mainSearchMC = new MainSearchMC()
                {
                    Id = 23
                };
                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.LoadSMSTemplate,
                    Request = mainSearchMC
                };
               var sMSTemplates = Helpers.ExcutePostAPI<CORE.DTOs.MotorClaim.SMSTemplates>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

                sMSTemplates.ArSMS = sMSTemplates.ArSMS.Replace("{NAJM}", claims.FirstOrDefault().claims.AccidentNo).Replace("{REF}", claims.FirstOrDefault().claims.ClaimNo).Replace("{REASON}", claimant.StatusReason);

                SMSInput sMSInput = new SMSInput()
                {
                    Mobile = claimant.MobileNo,
                    MessageBody = sMSTemplates.ArSMS,
                    message="1"
                };

                sMSInput = Helpers.ExcutePostAPI<SMSInput>(sMSInput, _appSettings.APIHubPrefix + "api/MotorClaim/SendSMSAICC");

         
                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimants,
                    Request = claimant
                };
                claimant = Helpers.ExcutePostAPI<Claimants>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
                Helpers.RegisterHistory(_appSettings, claimant.ClaimId, "Update Claim status to " + ((Enums.ClaimantStatus)ClaimStatus).ToString(), HttpContext.Session.getSessionData<Users>("LoggedUser").UserName, claimant.Id);

            }


            claimant.ClaimantStatus = ClaimStatus;
            claimant.StatusReason = RejectionReason;




            if (ClaimStatus == (int)Enums.ClaimantStatus.Operation && (!claims.FirstOrDefault().claims.PremiaClaimId.HasValue || claims.FirstOrDefault().claims.PremiaClaimId.Value == 0))
            {

                PremiaIntegration.CreateClaim(claims.FirstOrDefault(), _appSettings, out Result, out error);
                PremiaIntegration.CreateTPClaim(claims.FirstOrDefault(), _appSettings, out Result, out error);
                //PremiaIntegration.CreateClaimEstimation(claims.FirstOrDefault(), _appSettings, out Result, out error);
                if (Result)
                {
                    setupClaimsRequestcs = new SetupClaimsRequestcs()
                    {
                        TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimants,
                        Request = claimant
                    };
                    claimant = Helpers.ExcutePostAPI<Claimants>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
                    Helpers.RegisterHistory(_appSettings, claimant.ClaimId, "Update Claim status to " + ((Enums.ClaimantStatus)ClaimStatus).ToString(), HttpContext.Session.getSessionData<Users>("LoggedUser").UserName, claimant.Id);

                }

            }
            return RedirectToAction("OperationEntry", new { obj = obj, err = error, Result = Result });
        }


        public IActionResult UpdateEClaim(string obj)
        {
            eClaims claimSearchobj = Helpers.Deserilize<eClaims>(Helpers.Decryption(obj));
            if (claimSearchobj.InsuranceSurveyed == (int)Enums.ClaimReportType.Basher)
            {
                try
                {
                    Int64 x = Convert.ToInt64(claimSearchobj.AccidentReport);
                }
                catch (Exception)
                {

                    claimSearchobj.InsuranceSurveyed = (int)Enums.ClaimReportType.Najm;
                }
            }
            ClaimSubmissionDocuments claimSubmissionDocuments = new ClaimSubmissionDocuments()
            {
                eClaimId = claimSearchobj.Id
            };
            claimSubmissionDocuments = Helpers.ExcutePostAPI<ClaimSubmissionDocuments>(claimSubmissionDocuments, _appSettings.APIHubPrefix + "api/MotorClaim/LoadeeClaimDocuments");
            NajmResponse najm = new NajmResponse();
            BasherResponseCode basherResponse = new BasherResponseCode();

            if (claimSearchobj.InsuranceSurveyed==(int)Enums.ClaimReportType.Najm)
            {   
                najm = Helpers.ExcutePostAPI<NajmResponse>(claimSearchobj.AccidentReport, _appSettings.APIHubPrefix + "api/MotorClaim/NajmDetails");
            }
            else if (claimSearchobj.InsuranceSurveyed == (int)Enums.ClaimReportType.Basher)
            {
                basherResponse = Helpers.ExcutePostAPI<BasherResponseCode>(Convert.ToInt64(claimSearchobj.AccidentReport), _appSettings.APIHubPrefix + "api/MotorClaim/BasherDetails");
            }
          
            ViewData["Attachments"] = Helpers.GeteClaimAttachment(claimSubmissionDocuments);
            ViewData["DocumentsLink"] = _appSettings.DocumentsLink;
            ViewData["Serial"] = claimSubmissionDocuments.Serial;
            ViewData["policyNumber"] = claimSearchobj.InsuranceSurveyed == (int)Enums.ClaimReportType.Najm? najm.partyInsuranceInfos.Where(p => p.insuranceCompanyID == _appSettings.insuranceCompanyID).FirstOrDefault()?.policyNumber: basherResponse.InvolvedVehicles.Where(p=>p.insuranceCompany== "شركة التأمين العربية التعاونية").FirstOrDefault()?.insurancePolicyNumber;
            return View("_eClaimsEntry", claimSearchobj);
        }

        [HttpPost]
        public IActionResult UpdateEclaimProccess(eClaims obj)
        {
            string PolicyNumber = HttpContext.Request.Form["PolicyNumber"];
            if (obj.InsuranceSurveyed == (int)Enums.ClaimReportType.Basher)
            {
                try
                {
                    Int64 x = Convert.ToInt64(obj.AccidentReport);
                }
                catch (Exception)
                {

                    obj.InsuranceSurveyed = (int)Enums.ClaimReportType.Najm;
                }
            }
            if (obj.IsRejected.HasValue && obj.IsRejected.Value && !string.IsNullOrEmpty(obj.MobileNo))
            {
                string Lang = HttpContext.Session.getSessionData<string>("Lang");
                string smsBody = string.Empty;
                if (Lang.ToUpper() == "EN")
                {
                    smsBody = "Dear Customer,\n Kindly note that your claim submission has been declined for below reason \n " + obj.RejectionReason;
                }
                else
                {
                    smsBody = "عزيزي العميل , \n لقد تم رفض المطالبة المقدمة من قبلكم للسبب التالي \n " + obj.RejectionReason;

                }
                Helpers.SendSms(obj.MobileNo, smsBody, _appSettings);
            }
            else
            {
                obj.MobileNo = PolicyNumber;
                var claim = Helpers.ExcutePostAPI<string>(obj, _appSettings.APIHubPrefix + "api/MotorClaim/UpdateEclaim");
            }

            return RedirectToAction("eClaims");
        }

        [HttpPost]
        public void DeleteAttachment(int Id)
        {
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = Id
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.DeleteAttachment,
                Request = mainSearchMC
            };
            var claim = Helpers.ExcutePostAPI<bool>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");
        }
    }
}
