using CORE.DTOs.APIs.Business;
using CORE.DTOs.APIs.MotorClaim;
using CORE.DTOs.APIs.Process.Approvals;
using CORE.DTOs.APIs.TP_Services;
using CORE.DTOs.Authentications;
using CORE.DTOs.MotorClaim;
using CORE.DTOs.MotorClaim.Claims;
using CORE.DTOs.MotorClaim.Productions;
using CORE.DTOs.MotorClaim.WorkFlow;
using CORE.DTOs.Setups;
using CORE.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MotorClaims.Models;
using System.Drawing;
using System.Security.Claims;
using X.PagedList;
using static MotorClaims.Models.Enums;

namespace MotorClaims.Controllers
{
    public class SurveyorController : Controller
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string VehicleListCacheKey = "LookupTable";
        private readonly IMemoryCache _memoryCache;
        public static IWebHostEnvironment _environment;
        List<LookupTable> query = new List<LookupTable>();

        public SurveyorController(IOptions<AppSettings> appSettings, IWebHostEnvironment environment, IMemoryCache memoryCache)
        {
            _environment = environment;
            _appSettings = appSettings.Value;
            _memoryCache = memoryCache;
            _memoryCache.TryGetValue(VehicleListCacheKey, out query);
        }

        [CustomAuthorize(Roles = "Admin,Surveyor,Opertion")]

        public IActionResult Index(int page = 1)
        {

            List<ClaimMaster> claim = new List<ClaimMaster>();
            claim = HttpContext.Session.getSessionData<List<ClaimMaster>>("SearchResult");
            IPagedList<ClaimMaster> claims = claim.ToPagedList(page, _appSettings.PageSize);
            ViewData["searchObj"] = new SearchObj();
            ViewData["AllUsers"] = HttpContext.Session.getSessionData<List<Users>>("AllUsers");
            return View(claims);
        }

        public IActionResult SurveyorEntry(string obj, string? err)
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

            mainSearchMC = new MainSearchMC()
            {
                ClaimId = Convert.ToInt32(claimSearchobj.ClaimId),
                ClaimantId = Convert.ToInt32(claimSearchobj.ClaimantId),

            };
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadSurveyor,
                Request = mainSearchMC
            };
            var survoyers = Helpers.ExcutePostAPI<List<Survoyer>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            HttpContext.Session.SetSessionData("survoyers", survoyers);
            ViewData["claimSearchobj"] = obj;
            ViewData["survoyers"] = survoyers;
            ViewData["ClaimId"] = claimSearchobj.ClaimId;
            ViewData["ClaimantId"] = claimSearchobj.ClaimantId;
            ViewData["IsWorkshop"] = claim.FirstOrDefault().vehiclesInfo.RepairCondition == (int)Enums.RepairConditions.Workshop;
            ViewData["Workshops"] = claim.FirstOrDefault().vehiclesInfo.RepairCondition == (int)Enums.RepairConditions.Workshop ? HttpContext.Session.getSessionData<List<Users>>("workshops") : HttpContext.Session.getSessionData<List<Users>>("Agencies");
            return View(claim.FirstOrDefault());


        }



        [HttpPost]
        public IActionResult SearchSurveyors(SearchObj searchObj)
        {
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
            var claim = Helpers.ExcutePostAPI<List<ClaimMaster>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            IPagedList<ClaimMaster> Surveyors = claim.ToPagedList(1, _appSettings.PageSize);
            HttpContext.Session.SetSessionData("SearchResult", claim);
            ViewData["searchObj"] = searchObj;
            ViewData["AllUsers"] = HttpContext.Session.getSessionData<List<Users>>("AllUsers");
            return View("Index", Surveyors);
        }

        [HttpPost]
        public IActionResult InsertSurveyor(Survoyer survoyer,
            [FromForm(Name = "fromFilesFront")] IFormFile Front, [FromForm(Name = "fromFilesRear")] IFormFile Rear,
            [FromForm(Name = "fromFilesLeft")] IFormFile Left, [FromForm(Name = "fromFilesRight")] IFormFile Right,
            [FromForm(Name = "fromFilesUnderfronthood")] IFormFile Underfronthood, [FromForm(Name = "fromFilesInterior")] IFormFile Interior,
            [FromForm(Name = "fromFilesUnderneath")] IFormFile Underneath, [FromForm(Name = "fromFilesOthers")] IFormFile Others)
        {
            Dictionary<IFormFile, string> Photos = new Dictionary<IFormFile, string>();
            if (Front != null && Front.Length > 0)
            {
                Photos.Add(Front, "Front");
            }
            if (Rear != null && Rear.Length > 0)
            {
                Photos.Add(Rear, "Rear");
            }
            if (Left != null && Left.Length > 0)
            {
                Photos.Add(Left, "Left");
            }
            if (Right != null && Right.Length > 0)
            {
                Photos.Add(Right, "Right");
            }
            if (Underfronthood != null && Underfronthood.Length > 0)
            {
                Photos.Add(Underfronthood, "Underfronthood");
            }
            if (Interior != null && Interior.Length > 0)
            {
                Photos.Add(Interior, "Interior");
            }
            if (Underneath != null && Underneath.Length > 0)
            {
                Photos.Add(Underneath, "Underneath");
            }
            if (Others != null && Others.Length > 0)
            {
                Photos.Add(Others, "Others");
            }
            string obj = HttpContext.Request.Form["claimSearchobj"].ToString();
            survoyer.CreationDate = DateTime.Now;
            survoyer.CreatedBy = HttpContext.Session.getSessionData<Users>("LoggedUser").UserName;
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateSurvoyerEntry,
                Request = survoyer
            };
            survoyer = Helpers.ExcutePostAPI<Survoyer>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            Helpers.HandleSurveyorPhoto(Photos, survoyer, HttpContext.Session.getSessionData<Users>("LoggedUser").UserName, _appSettings);

            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                ClaimId = Convert.ToInt32(survoyer.ClaimId),
                ClaimantId = survoyer.ClaimantId
            };
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.ReserveBalance,
                Request = mainSearchMC
            };

            var clm = Helpers.ExcutePostAPI<ReserveBalance>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            ClaimTransactions claimTransactions = new ClaimTransactions()
            {
                ClaimantID = Convert.ToInt32(HttpContext.Request.Form["ClaimantID"]),
                ClaimId = survoyer.ClaimId,
                CollectionType = string.Empty,
                Collector = 1,
                Commission = 0,
                Fees = 0,
                isActive = false,
                ParentTransactions = null,
                Payment = string.Empty,
                TransactionAmount = ((survoyer.LabourAmount.HasValue ? survoyer.LabourAmount.Value : 0) + (survoyer.OtherAmount.HasValue ? survoyer.OtherAmount.Value : 0) + (survoyer.SparePartAmount.HasValue ? survoyer.SparePartAmount.Value : 0)) - clm.ClaimantLevel,
                TransactionDate = DateTime.Now,
                TransactionType = (int)Enums.ClaimTransactionTypes.Reserve,
                CreatedBy = HttpContext.Session.getSessionData<Users>("LoggedUser").UserName,
                Note = survoyer.Note
            };
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimTransaction,
                Request = claimTransactions
            };
            claimTransactions = Helpers.ExcutePostAPI<ClaimTransactions>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            Reserve reserve = new Reserve()
            {
                ClaimantId = survoyer.ClaimantId,
                ClaimId = survoyer.ClaimId,
                CreatedBy = HttpContext.Session.getSessionData<Users>("LoggedUser").Id,
                CreatedByName = HttpContext.Session.getSessionData<Users>("LoggedUser").UserName,
                Creationdate = DateTime.Now,
                Note = survoyer.Note,
                Status = false,
                TransactionId = claimTransactions.Id
            };

            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertReserve,
                Request = reserve
            };
            reserve = Helpers.ExcutePostAPI<Reserve>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            ReserveDetails reserveDetails = new ReserveDetails();
            if (survoyer.SparePartAmount.HasValue && survoyer.SparePartAmount.Value > 0)
            {
                reserveDetails = new ReserveDetails()
                {
                    IsSettled = false,
                    ReserveId = reserve.Id,
                    ReserveType = (int)Enums.ReserveType.SpareParts,
                    Amount = survoyer.SparePartAmount.Value
                };
                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertReserveDetail,
                    Request = reserveDetails
                };
                reserveDetails = Helpers.ExcutePostAPI<ReserveDetails>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            }
            if (survoyer.LabourAmount.HasValue && survoyer.LabourAmount.Value > 0)
            {
                reserveDetails = new ReserveDetails()
                {
                    IsSettled = false,
                    ReserveId = reserve.Id,
                    ReserveType = (int)Enums.ReserveType.LaborCost,
                    Amount = survoyer.LabourAmount.Value
                };
                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertReserveDetail,
                    Request = reserveDetails
                };
                reserveDetails = Helpers.ExcutePostAPI<ReserveDetails>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            }
            if (survoyer.OtherAmount.HasValue && survoyer.OtherAmount.Value > 0)
            {
                reserveDetails = new ReserveDetails()
                {
                    IsSettled = false,
                    ReserveId = reserve.Id,
                    ReserveType = (int)Enums.ReserveType.OtherCost,
                    Amount = survoyer.OtherAmount.Value
                };
                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertReserveDetail,
                    Request = reserveDetails
                };
                reserveDetails = Helpers.ExcutePostAPI<ReserveDetails>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            }
            if (claimTransactions.TransactionAmount > 0 || claimTransactions.TransactionAmount < 0)
            {
                Helpers.PublishWorkflow(claimTransactions.TransactionAmount, Enums.WorkflowType.Reserve, claimTransactions.Id, claimTransactions.ClaimId, claimTransactions.ClaimantID, claimTransactions.CreatedBy, HttpContext.Session.getSessionData<List<Users>>("AllUsers"), _appSettings);
            }


            if (survoyer.Status == (int)Enums.SurvorStatus.TotalLoss)
            {
                Claimants claimants = new Claimants();
                mainSearchMC = new MainSearchMC()
                {
                    Id = survoyer.ClaimantId
                };
                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimants,
                    Request = mainSearchMC
                };
                var claimantsList = Helpers.ExcutePostAPI<List<Claimants>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
                claimants = claimantsList.FirstOrDefault();
                claimants.ClaimantStatus = (int)Enums.ClaimantStatus.WaitingSalvageApprovals;
                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimants,
                    Request = claimants
                };
                claimants = Helpers.ExcutePostAPI<Claimants>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

                Helpers.PublishWorkflow(claimTransactions.TransactionAmount + clm.ClaimantLevel, Enums.WorkflowType.TotalLoss, claimTransactions.Id, claimTransactions.ClaimId, claimTransactions.ClaimantID, claimTransactions.CreatedBy, HttpContext.Session.getSessionData<List<Users>>("AllUsers"), _appSettings);

            }
            return RedirectToAction("SurveyorEntry", new { obj = obj });
        }

        [HttpPost]
        public async void UploadAttachments(int Id, int ClaimId, int ClaimantId, int ModuleId, IList<IFormFile> files)
        {

            if (files != null && files.Count > 0)
            {
                Attachments attachment = new Attachments()
                {
                    ClaimantId = ClaimantId,
                    ClaimId = ClaimId,
                    CreationDate = DateTime.Now,
                    DocumentSetupId = Id,
                    ModuleId = ModuleId,
                    FileName = Path.GetFileName(files[0].FileName),
                    ContentType = files[0].ContentType,
                    CreatedBy = "Test",
                    IsDeleted = false,
                };
                SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateAttachment,
                    Request = attachment
                };
                attachment = Helpers.ExcutePostAPI<Attachments>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

                foreach (IFormFile file in files)
                {
                    string pathMDF = _appSettings.ExcelPath;
                    string fieNameWithExt = attachment.Id.ToString() + "_" + Path.GetFileName(file.FileName);
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    if (file.Length > 0)
                    {
                        string directory = Path.Combine(pathMDF, attachment.ClaimantId.ToString());
                        bool folderExists = Directory.Exists(directory);
                        if (!folderExists)
                            Directory.CreateDirectory(directory);

                        string filePath = Path.Combine(directory, fieNameWithExt);
                        using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                    }
                }

            }

        }

        public IActionResult SurveyorAssign(int ClaimId, int ClaimantId)
        {
            ViewData["DivName"] = "SurveyorAssign";
            ViewData["AllUsers"] = HttpContext.Session.getSessionData<List<Users>>("AllUsers");
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
            return View("_SurveyorAssign", claim.FirstOrDefault());
        }

        public IActionResult SurveyorActions(int ClaimId, int ClaimantId)
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
            return View(claim.FirstOrDefault());
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
            ViewData["ReserveCodes"] = query.Where(p => p.MajorCode == (int)Lookups.ReserveCodes).ToList();
            return View("_UpdateReserve", claim.FirstOrDefault());
        }

        [HttpPost]
        public void UpdateClaimantActions()
        {
            int ClaimId = Convert.ToInt32(HttpContext.Request.Form["Id"]);
            int ClaimantId = Convert.ToInt32(HttpContext.Request.Form["ClaimantId"]);
            int SurveyorActions = Convert.ToInt32(HttpContext.Request.Form["SurveyorActions"]);
            string hfSurveyorActions = HttpContext.Request.Form["hfSurveyorActions"];
            Claimants claimants = new Claimants();

            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                ClaimId = ClaimId,
                ClaimantId = ClaimantId
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimants,
                Request = mainSearchMC
            };

            var clm = Helpers.ExcutePostAPI<List<Claimants>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            claimants = clm.FirstOrDefault();

            claimants.LastClaimantAction = SurveyorActions;

            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimants,
                Request = claimants
            };
            claimants = Helpers.ExcutePostAPI<Claimants>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");


            Helpers.RegisterHistory(_appSettings, ClaimId, "Change Claimant Action To " + hfSurveyorActions, HttpContext.Session.getSessionData<Users>("LoggedUser").UserName, ClaimantId);

        }


        [HttpPost]
        public IActionResult UpdateReserveTransaction(Reserve reserve)
        {
            if (Helpers.CheckPendingApprovals(reserve.ClaimantId, (int)Enums.WorkflowType.Reserve, _appSettings))
            {
                return RedirectToAction("UpdateReserve", "Operations", new { ClaimId = reserve.ClaimId, ClaimantId = reserve.ClaimantId, err = "There are pending Approval" });
            }

            Users LoggedUser = HttpContext.Session.getSessionData<Users>("LoggedUser");
            reserve.Creationdate = DateTime.Now;
            reserve.Status = false;

            string? PremiaCode1 = HttpContext.Request.Form["PremiaCode1"];
            string? PremiaCode2 = HttpContext.Request.Form["PremiaCode2"];
            string? PremiaCode3 = HttpContext.Request.Form["PremiaCode3"];
            string? PremiaCode4 = HttpContext.Request.Form["PremiaCode4"];
            string? PremiaCode5 = HttpContext.Request.Form["PremiaCode5"];


            decimal? SparePartCost = !string.IsNullOrEmpty(HttpContext.Request.Form["SparePartCost"]) ? Convert.ToDecimal(HttpContext.Request.Form["SparePartCost"]) : 0;
            decimal? LaborCost = !string.IsNullOrEmpty(HttpContext.Request.Form["LaborCost"]) ? Convert.ToDecimal(HttpContext.Request.Form["LaborCost"]) : 0;
            decimal? SheikhCost = !string.IsNullOrEmpty(HttpContext.Request.Form["SheikhCost"]) ? Convert.ToDecimal(HttpContext.Request.Form["SheikhCost"]) : 0;
            decimal? TowingCost = !string.IsNullOrEmpty(HttpContext.Request.Form["TowingCost"]) ? Convert.ToDecimal(HttpContext.Request.Form["TowingCost"]) : 0;
            decimal? OtherCost = !string.IsNullOrEmpty(HttpContext.Request.Form["OtherCost"]) ? Convert.ToDecimal(HttpContext.Request.Form["OtherCost"]) : 0;
            List<ReserveDetails> reserveDetails = new List<ReserveDetails>();

            //MainSearchMC mainSearchMC = new MainSearchMC()
            //{
            //    ClaimId = Convert.ToInt32(reserve.ClaimId),
            //    ClaimantId = reserve.ClaimantId
            //};
            //SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            //{
            //    TransactionType = CORE.Extensions.ClaimTransactionType.ReserveBalance,
            //    Request = mainSearchMC
            //};

            //var clm = Helpers.ExcutePostAPI<ReserveBalance>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            bool CheckAuthority = LoggedUser.EstimateAuthority > (SparePartCost + OtherCost + LaborCost + SheikhCost + TowingCost) ? true : false;





            ClaimTransactions claimTransactions = new ClaimTransactions()
            {
                ClaimantID = Convert.ToInt32(HttpContext.Request.Form["ClaimantID"]),
                ClaimId = reserve.ClaimId,
                CollectionType = string.Empty,
                Collector = 1,
                Commission = 0,
                Fees = 0,
                isActive = CheckAuthority,
                ParentTransactions = null,
                Payment = string.Empty,
                TransactionAmount = (SparePartCost.Value + OtherCost.Value + LaborCost.Value + TowingCost.Value + SheikhCost.Value),
                TransactionDate = DateTime.Now,
                TransactionType = (int)Enums.ClaimTransactionTypes.Reserve,
                CreatedBy = HttpContext.Session.getSessionData<Users>("LoggedUser").UserName,
                Note = reserve.Note
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimTransaction,
                Request = claimTransactions
            };
            claimTransactions = Helpers.ExcutePostAPI<ClaimTransactions>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            reserve.CreatedBy = HttpContext.Session.getSessionData<Users>("LoggedUser").Id;
            reserve.CreatedByName = HttpContext.Session.getSessionData<Users>("LoggedUser").UserName;
            reserve.TransactionId = claimTransactions.Id;
            reserve.Status = CheckAuthority;

            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertReserve,
                Request = reserve
            };
            reserve = Helpers.ExcutePostAPI<Reserve>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            ReserveDetails reserveDetail = new ReserveDetails();
            if (!string.IsNullOrEmpty(PremiaCode1) && SparePartCost.HasValue && SparePartCost.Value>0)
            {
                reserveDetail = new ReserveDetails()
                {
                    Amount = SparePartCost.Value,
                    IsSettled = false,
                    PremiaClaimId = 0,
                    PremiaCode = PremiaCode1,
                    ReserveType = (int)Enums.ReserveType.SpareParts,
                    ReserveId = reserve.Id
                };
         
                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertReserveDetail,
                    Request = reserveDetail
                };
                reserveDetail = Helpers.ExcutePostAPI<ReserveDetails>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
                reserveDetails.Add(reserveDetail);
            }
            if (!string.IsNullOrEmpty(PremiaCode2) && LaborCost.HasValue && LaborCost.Value > 0)
            {
                reserveDetail = new ReserveDetails()
                {
                    Amount = LaborCost.Value,
                    IsSettled = false,
                    PremiaClaimId = 0,
                    PremiaCode = PremiaCode2,
                    ReserveType = (int)Enums.ReserveType.LaborCost,
                    ReserveId = reserve.Id
                };

                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertReserveDetail,
                    Request = reserveDetail
                };
                reserveDetail = Helpers.ExcutePostAPI<ReserveDetails>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
                reserveDetails.Add(reserveDetail);

            }
            if (!string.IsNullOrEmpty(PremiaCode3) && OtherCost.HasValue && OtherCost.Value > 0)
            {
                reserveDetail = new ReserveDetails()
                {
                    Amount = OtherCost.Value,
                    IsSettled = false,
                    PremiaClaimId = 0,
                    PremiaCode = PremiaCode3,
                    ReserveType = (int)Enums.ReserveType.OtherCost,
                    ReserveId = reserve.Id
                };

                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertReserveDetail,
                    Request = reserveDetail
                };
                reserveDetail = Helpers.ExcutePostAPI<ReserveDetails>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
                reserveDetails.Add(reserveDetail);

            }
            if (!string.IsNullOrEmpty(PremiaCode4) && SheikhCost.HasValue && SheikhCost.Value > 0 )
            {
                reserveDetail = new ReserveDetails()
                {
                    Amount = SheikhCost.Value,
                    IsSettled = false,
                    PremiaClaimId = 0,
                    PremiaCode = PremiaCode4,
                    ReserveType = (int)Enums.ReserveType.SheikhAlmaared,
                    ReserveId = reserve.Id
                };

                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertReserveDetail,
                    Request = reserveDetail
                };
                reserveDetail = Helpers.ExcutePostAPI<ReserveDetails>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
                reserveDetails.Add(reserveDetail);

            }
            if (!string.IsNullOrEmpty(PremiaCode5) && TowingCost.HasValue && TowingCost.Value > 0)
            {
                reserveDetail = new ReserveDetails()
                {
                    Amount = TowingCost.Value,
                    IsSettled = false,
                    PremiaClaimId = 0,
                    PremiaCode = PremiaCode5,
                    ReserveType = (int)Enums.ReserveType.TowingCost,
                    ReserveId = reserve.Id
                };

                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertReserveDetail,
                    Request = reserveDetail
                };
                reserveDetail = Helpers.ExcutePostAPI<ReserveDetails>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
                reserveDetails.Add(reserveDetail);

            }


            bool Result = false; string error = string.Empty;
            if (!CheckAuthority && LoggedUser.EstimateAuthority <  (SparePartCost.Value  + OtherCost.Value +LaborCost.Value +SheikhCost.Value+TowingCost.Value))
            {
                Helpers.PublishWorkflow(claimTransactions.TransactionAmount, Enums.WorkflowType.Reserve, claimTransactions.Id, claimTransactions.ClaimId, claimTransactions.ClaimantID, claimTransactions.CreatedBy, HttpContext.Session.getSessionData<List<Users>>("AllUsers"), _appSettings);
                Helpers.RegisterHistory(_appSettings, reserve.ClaimId, "Update Reserve to " + (SparePartCost.Value + OtherCost.Value + LaborCost.Value + SheikhCost.Value + TowingCost.Value) + " SAR by " + HttpContext.Session.getSessionData<Users>("LoggedUser").UserName + " Pending with Approval", HttpContext.Session.getSessionData<Users>("LoggedUser").UserName, claimTransactions.ClaimantID);

            }
            else
            {
                MainSearchMC mainSearchMC = new MainSearchMC()
                {
                    Id = Convert.ToInt32(reserve.ClaimId),
                    ClaimantId = reserve.ClaimantId

                };
                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
                    Request = mainSearchMC
                };
                var claim = Helpers.ExcutePostAPI<List<ClaimMaster>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");



                //Close previouse reserve
                //var reserve2=claim.FirstOrDefault().reserve.Where(p=>p.Id!=reserve.Id).FirstOrDefault();
                //if (reserve2 != null)
                //{
                //    if (reserve2.PremiaClaimId1.HasValue && reserve2.PremiaClaimId1.Value>0)
                //    {
                //        PremiaIntegration.CreateClaimEstimation(claim.FirstOrDefault(), reserve2, reserve2.SparePartCost.Value, 1, true, _appSettings, out Result, out error);
                //    }
                //    if (reserve2.PremiaClaimId2.HasValue && reserve2.PremiaClaimId2.Value > 0)
                //    {
                //        PremiaIntegration.CreateClaimEstimation(claim.FirstOrDefault(), reserve2, reserve2.LaborCost.Value, 2, true, _appSettings, out Result, out error);
                //    }
                //    if (reserve2.PremiaClaimId3.HasValue && reserve2.PremiaClaimId3.Value > 0)
                //    {
                //        PremiaIntegration.CreateClaimEstimation(claim.FirstOrDefault(), reserve2, reserve2.OtherCost.Value, 3, true, _appSettings, out Result, out error);
                //    }
                //}
                string ReserveCode = string.Empty;
                ReserveObj reserveObj=new ReserveObj()
                {
                    reserve = reserve,
                    reserveDetails=reserveDetails
                };
                if (SparePartCost.HasValue && SparePartCost.Value > 0 && !string.IsNullOrEmpty(PremiaCode1))
                {

                    PremiaIntegration.CreateClaimEstimation(claim.FirstOrDefault(), reserveObj, SparePartCost.Value, 1, false, _appSettings, out Result, out error);

                    if (!Result)
                    {
                        rollbackReserve(reserve.Id, claimTransactions.Id);
                    }
                }
                if (LaborCost.HasValue && LaborCost.Value > 0 && !string.IsNullOrEmpty(PremiaCode2)) 
                {

                    PremiaIntegration.CreateClaimEstimation(claim.FirstOrDefault(), reserveObj, LaborCost.Value, 2, false, _appSettings, out Result, out error);
                    if (!Result)
                    {
                        rollbackReserve(reserve.Id, claimTransactions.Id);
                    }
                }
                if (OtherCost.HasValue && OtherCost.Value > 0 && !string.IsNullOrEmpty(PremiaCode3))
                {
                    PremiaIntegration.CreateClaimEstimation(claim.FirstOrDefault(), reserveObj, OtherCost.Value, 3, false, _appSettings, out Result, out error);
                    if (!Result)
                    {
                        rollbackReserve(reserve.Id, claimTransactions.Id);
                    }
                }
                if (SheikhCost.HasValue && SheikhCost.Value > 0 && !string.IsNullOrEmpty(PremiaCode4))
                {
                    PremiaIntegration.CreateClaimEstimation(claim.FirstOrDefault(), reserveObj, SheikhCost.Value, 4, false, _appSettings, out Result, out error);
                    if (!Result)
                    {
                        rollbackReserve(reserve.Id, claimTransactions.Id);
                    }
                }
                if (TowingCost.HasValue && TowingCost.Value > 0 && !string.IsNullOrEmpty(PremiaCode5))
                {
                    PremiaIntegration.CreateClaimEstimation(claim.FirstOrDefault(), reserveObj, TowingCost.Value, 5, false, _appSettings, out Result, out error);
                    if (!Result)
                    {
                      rollbackReserve(reserve.Id, claimTransactions.Id);
                    }
                }
                Helpers.RegisterHistory(_appSettings, reserve.ClaimId, "Update Reserve by " + (SparePartCost.Value  + OtherCost.Value + LaborCost.Value  + TowingCost.Value + SheikhCost.Value ) + " SAR by " + HttpContext.Session.getSessionData<Users>("LoggedUser").UserName, HttpContext.Session.getSessionData<Users>("LoggedUser").UserName, claimTransactions.ClaimantID);

            }

            ViewData["ReserveCodes"] = query.Where(p => p.MajorCode == (int)Lookups.ReserveCodes).ToList();
            return View("_UpdateReserve", HttpContext.Session.getSessionData<ClaimMaster>("ClaimMasterReserve"));
        }


        public  void rollbackReserve(int ReserveId, int ClaimTransactionId)
        {
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                ClaimId = ReserveId,
                ClaimantId = ClaimTransactionId
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.ReserveRollback,
                Request = mainSearchMC
            };

            var clm = Helpers.ExcutePostAPI<bool>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
        }

        [HttpPost]
        public void UpdateRecoveryTransaction(ClaimRecoveries recoveries)
        {
            recoveries.CreatedBy = HttpContext.Session.getSessionData<Users>("LoggedUser").UserName;
            recoveries.CreationDate = DateTime.Now;
            recoveries.Status = 1;

            ClaimTransactions claimTransactions = new ClaimTransactions()
            {
                ClaimantID = recoveries.ClaimantId,
                ClaimId = recoveries.ClaimId,
                CollectionType = string.Empty,
                Collector = 1,
                Commission = 0,
                Fees = 0,
                isActive = true,
                ParentTransactions = null,
                Payment = string.Empty,
                TransactionAmount = recoveries.RecoveryAmount,
                TransactionDate = DateTime.Now,
                TransactionType = (int)Enums.ClaimTransactionTypes.Recovery,
                CreatedBy = recoveries.CreatedBy,
                Note = recoveries.Note
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimTransaction,
                Request = claimTransactions
            };
            claimTransactions = Helpers.ExcutePostAPI<ClaimTransactions>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            recoveries.ClaimTransactionId = claimTransactions.Id;
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertClaimRecovery,
                Request = recoveries
            };
            recoveries = Helpers.ExcutePostAPI<ClaimRecoveries>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            Helpers.RegisterHistory(_appSettings, recoveries.ClaimId, "Add Recovery " + claimTransactions.TransactionAmount + " SAR by " + HttpContext.Session.getSessionData<Users>("LoggedUser").UserName, HttpContext.Session.getSessionData<Users>("LoggedUser").UserName, claimTransactions.ClaimantID);


        }
        //public IActionResult LoadSurveyorsList(string Id)
        //{
        //    ClaimSearchobj claimSearchobj = Helpers.Deserilize<ClaimSearchobj>(Helpers.Decryption(Id));
        //    MainSearchMC mainSearchMC = new MainSearchMC()
        //    {
        //        ClaimId = Convert.ToInt32(claimSearchobj.ClaimId),
        //        ClaimantId = Convert.ToInt32(claimSearchobj.ClaimantId),

        //    };
        //    SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
        //    {
        //        TransactionType = CORE.Extensions.ClaimTransactionType.LoadSurveyor,
        //        Request = mainSearchMC
        //    };
        //    var survoyers = Helpers.ExcutePostAPI<List<Survoyer>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

        //    HttpContext.Session.SetSessionData("survoyers", survoyers);
        //    ViewData["claimSearchobj"] = Id;
        //    ViewData["survoyers"] = survoyers;
        //    ViewData["ClaimId"] = claimSearchobj.ClaimId;
        //    ViewData["ClaimantId"] = claimSearchobj.ClaimantId;
        //    return View("_SurveyorsList");
        //}

        public IActionResult LoadSurveyorEntry(string Id)
        {
            ClaimSearchobj claimSearchobj = Helpers.Deserilize<ClaimSearchobj>(Helpers.Decryption(Id));
            if (!claimSearchobj.SurvoyerId.HasValue && Helpers.CheckPendingApprovals(claimSearchobj.ClaimantId.Value, 0, _appSettings))
            {

                return RedirectToAction("SurveyorEntry", "Surveyor", new { obj = Id, err = "There are pending Approval" });
            }

            List<Survoyer> survoyers = new List<Survoyer>();
            survoyers = HttpContext.Session.getSessionData<List<Survoyer>>("survoyers");
            ViewData["survoyers"] = survoyers;
            Survoyer survoyer = new Survoyer();
            if (survoyers.Count > 0 && claimSearchobj.SurvoyerId > 0)
            {
                survoyer = survoyers.Where(p => p.Id == claimSearchobj.SurvoyerId).FirstOrDefault();
            }
            else
            {
                survoyer.ClaimantId = claimSearchobj.ClaimantId.Value;
                survoyer.ClaimId = claimSearchobj.ClaimId.Value;
            }
            ViewData["claimSearchobj"] = Id;

            return View("_SurveyorEntry", survoyer);
        }

        public IActionResult LoadSurveyorMissing(string Id)
        {
            ClaimSearchobj claimSearchobj = Helpers.Deserilize<ClaimSearchobj>(Helpers.Decryption(Id));
            List<MissingParts> missingParts = new List<MissingParts>();

            MissingParts survoyer = new MissingParts();
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                ClaimId = Convert.ToInt32(claimSearchobj.ClaimId),
                ClaimantId = Convert.ToInt32(claimSearchobj.ClaimantId),
                SurveyId = claimSearchobj.SurvoyerId

            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadMissingParts,
                Request = mainSearchMC
            };
            missingParts = Helpers.ExcutePostAPI<List<MissingParts>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");
            if (missingParts.Count > 0 && claimSearchobj.SurvoyerId > 0)
            {
                survoyer = missingParts.Where(p => p.SurveyId == claimSearchobj.SurvoyerId).FirstOrDefault();
            }
            else
            {
                survoyer.ClaimantId = claimSearchobj.ClaimantId.Value;
                survoyer.ClaimId = claimSearchobj.ClaimId.Value;
                survoyer.SurveyId = claimSearchobj.SurvoyerId.Value;
            }
            List<Survoyer> survoyers = HttpContext.Session.getSessionData<List<Survoyer>>("survoyers");
            ViewData["survoyers"] = survoyers;
            ViewData["claimSearchobj"] = Id;
            return View("_SurveyorMissing", survoyer);
        }

        [HttpPost]
        public IActionResult InsertSurveyorMissing(MissingParts survoyer)
        {
            string obj = HttpContext.Request.Form["claimSearchobj"].ToString();
            survoyer.CreationDate = DateTime.Now;
            survoyer.CreatedBy = HttpContext.Session.getSessionData<Users>("LoggedUser").UserName;
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertMissingParts,
                Request = survoyer
            };
            survoyer = Helpers.ExcutePostAPI<MissingParts>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            return RedirectToAction("SurveyorEntry", new { obj = obj });
        }

        [HttpPost]
        public IActionResult UpdateCollectionTransaction(ClaimRecoveries claimRecoveries, [FromForm(Name = "fromFiles")] IFormFile Invoice)
        {

            ClaimTransactions claimTransactions = new ClaimTransactions()
            {
                ClaimantID = claimRecoveries.ClaimantId,
                ClaimId = claimRecoveries.ClaimId,
                CollectionType = string.Empty,
                Collector = 1,
                Commission = 0,
                Fees = 0,
                isActive = true,
                ParentTransactions = claimRecoveries.ClaimTransactionId,
                Payment = string.Empty,
                TransactionAmount = claimRecoveries.RecoveryAmount,
                TransactionDate = DateTime.Now,
                TransactionType = (int)Enums.ClaimTransactionTypes.Collection,
                CreatedBy = HttpContext.Session.getSessionData<Users>("LoggedUser").UserName,
                Note = claimRecoveries.Note
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimTransaction,
                Request = claimTransactions
            };
            claimTransactions = Helpers.ExcutePostAPI<ClaimTransactions>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            //recoveries.ClaimTransactionId = claimTransactions.Id;
            //setupClaimsRequestcs = new SetupClaimsRequestcs()
            //{
            //    TransactionType = CORE.Extensions.ClaimTransactionType.InsertClaimRecovery,
            //    Request = recoveries
            //};
            //recoveries = Helpers.ExcutePostAPI<ClaimRecoveries>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            if (Invoice != null && Invoice.Length > 0)
            {
                string filePath = Path.Combine("", Invoice.FileName);
                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    Invoice.CopyTo(fileStream);
                }

            }

            return RedirectToAction("RegisterCollection", "Collections", new { obj = HttpContext.Session.getSessionData<string>("RecoveryObj") });
        }
        [CustomAuthorize(Roles = "Admin,Workshops,Opertion")]
        public IActionResult Workshops(string? err = null, int page = 1)
        {
            if (!string.IsNullOrEmpty(err))
            {
                ViewData["Error"] = err;
            }
            List<ClaimMaster> claim = new List<ClaimMaster>();
            claim = HttpContext.Session.getSessionData<List<ClaimMaster>>("SearchResult");
            IPagedList<ClaimMaster> claims = claim.ToPagedList(page, _appSettings.PageSize);
            ViewData["searchObj"] = new SearchObj();
            ViewData["AllUsers"] = HttpContext.Session.getSessionData<List<Users>>("AllUsers");
            return View(claims);
        }

        public IActionResult WorkshopEntry(string obj, string? err)
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


            mainSearchMC = new MainSearchMC()
            {
                ClaimId = Convert.ToInt32(claimSearchobj.ClaimId),
                ClaimantId = Convert.ToInt32(claimSearchobj.ClaimantId),

            };
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadSurveyor,
                Request = mainSearchMC
            };
            var survoyers = Helpers.ExcutePostAPI<List<Survoyer>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            HttpContext.Session.SetSessionData("survoyers", survoyers);
            ViewData["claimSearchobj"] = obj;
            ViewData["ClaimId"] = claimSearchobj.ClaimId;
            ViewData["ClaimantId"] = claimSearchobj.ClaimantId;
            ViewData["query"] = query;
            //string FileName = claim.FirstOrDefault().attachments.Where(p => p.DocumentSetupId == 1014).OrderByDescending(p => p.Id).FirstOrDefault().FileName.Replace("//", "/");
            ViewData["Link"] = _appSettings.ClaimSubmissionURL + claim.FirstOrDefault().claims.ClaimNo + "/" + claim.FirstOrDefault().claimants.Serial.ToString() + "/";
            return View(claim.FirstOrDefault());


        }

        [HttpPost]
        public IActionResult SearchWorkshops(SearchObj searchObj)
        {
            //if (string.IsNullOrEmpty(searchObj.nationalid) && !searchObj.RegisteredFrom.HasValue && !searchObj.RegisteredTo.HasValue && string.IsNullOrEmpty(searchObj.chassis) && string.IsNullOrEmpty(searchObj.claimno) && string.IsNullOrEmpty(searchObj.mobile) && string.IsNullOrEmpty(searchObj.policy))
            //{
            //    return RedirectToAction("Workshops", new { err = "Please fill at least one parameter" });
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
                UserId = HttpContext.Session.getSessionData<Users>("LoggedUser").Id

            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
                Request = mainSearchMC
            };
            var claim = Helpers.ExcutePostAPI<List<ClaimMaster>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            IPagedList<ClaimMaster> Surveyors = claim.ToPagedList(1, _appSettings.PageSize);
            HttpContext.Session.SetSessionData("SearchResult", claim);
            ViewData["searchObj"] = searchObj;
            ViewData["AllUsers"] = HttpContext.Session.getSessionData<List<Users>>("AllUsers");
            return View("Workshops", Surveyors);
        }


        public IActionResult WorkshopActions(int ClaimId, int ClaimantId)
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
            return View(claim.FirstOrDefault());
        }


        [HttpPost]
        public IActionResult InsertWorkshopEntry(Survoyer survoyer,
    [FromForm(Name = "fromFilesFront")] IFormFile Front, [FromForm(Name = "fromFilesRear")] IFormFile Rear,
    [FromForm(Name = "fromFilesLeft")] IFormFile Left, [FromForm(Name = "fromFilesRight")] IFormFile Right,
    [FromForm(Name = "fromFilesUnderfronthood")] IFormFile Underfronthood, [FromForm(Name = "fromFilesInterior")] IFormFile Interior,
    [FromForm(Name = "fromFilesUnderneath")] IFormFile Underneath, [FromForm(Name = "fromFilesOthers")] IFormFile Others)
        {
            Dictionary<IFormFile, string> Photos = new Dictionary<IFormFile, string>();
            if (Front != null && Front.Length > 0)
            {
                Photos.Add(Front, "Front");
            }
            if (Rear != null && Rear.Length > 0)
            {
                Photos.Add(Rear, "Rear");
            }
            if (Left != null && Left.Length > 0)
            {
                Photos.Add(Left, "Left");
            }
            if (Right != null && Right.Length > 0)
            {
                Photos.Add(Right, "Right");
            }
            if (Underfronthood != null && Underfronthood.Length > 0)
            {
                Photos.Add(Underfronthood, "Underfronthood");
            }
            if (Interior != null && Interior.Length > 0)
            {
                Photos.Add(Interior, "Interior");
            }
            if (Underneath != null && Underneath.Length > 0)
            {
                Photos.Add(Underneath, "Underneath");
            }
            if (Others != null && Others.Length > 0)
            {
                Photos.Add(Others, "Others");
            }
            string obj = HttpContext.Request.Form["claimSearchobj"].ToString();
            survoyer.CreationDate = DateTime.Now;
            survoyer.CreatedBy = HttpContext.Session.getSessionData<Users>("LoggedUser").UserName;
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateSurvoyerEntry,
                Request = survoyer
            };
            survoyer = Helpers.ExcutePostAPI<Survoyer>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            Helpers.HandleSurveyorPhoto(Photos, survoyer, HttpContext.Session.getSessionData<Users>("LoggedUser").UserName, _appSettings);

            AutoAssignObj autoAssignObj = new AutoAssignObj()
            {
                ClaimantId = survoyer.ClaimantId,
                RoleId = (int)Enums.ClaimantStatus.Surveyor,
                Status = 2
            };
            var clmnt = Helpers.ExcutePostAPI<Claimants>(autoAssignObj, _appSettings.APIHubPrefix + "api/MotorClaim/AutoAssign");


            Helpers.RegisterHistory(_appSettings, survoyer.ClaimId, "Set Workshop Estimation and assign to " + HttpContext.Session.getSessionData<List<Users>>("AllUsers").Where(p => p.Id == clmnt.AssignTo).FirstOrDefault().UserName, HttpContext.Session.getSessionData<Users>("LoggedUser").UserName, survoyer.ClaimantId);
            return RedirectToAction("WorkshopEntry", new { obj = obj });
        }

    }
}
