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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MotorClaims.Models;
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

        public IActionResult Index(int page=1)
        {
            List<ClaimMaster> claim = new List<ClaimMaster>();
            claim = HttpContext.Session.getSessionData<List<ClaimMaster>>("SearchResult");
            IPagedList<ClaimMaster> claims = claim.ToPagedList(page, _appSettings.PageSize);

            return View(claims);
        }

        public IActionResult SurveyorEntry(string obj)
        {
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

           
            return View(claim.FirstOrDefault());
        }

        [HttpPost]
        public IActionResult SearchSurveyors(int page=1)
        {
            MainSearchMC mainSearchMC = new MainSearchMC();
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
                Request = mainSearchMC
            };
            var claim = Helpers.ExcutePostAPI<List<ClaimMaster>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            IPagedList<ClaimMaster> Surveyors = claim.ToPagedList(page, _appSettings.PageSize);
            HttpContext.Session.SetSessionData("SearchResult", claim);
            return View("Index", Surveyors);
        }

        [HttpPost]
        public IActionResult InsertSurveyor(Survoyer survoyer)
        {
            string obj = HttpContext.Request.Form["claimSearchobj"].ToString();
            survoyer.CreationDate = DateTime.Now;
            survoyer.CreatedBy = HttpContext.Session.getSessionData<Users>("LoggedUser").UserName;
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateSurvoyerEntry,
                Request = survoyer
            };
            survoyer = Helpers.ExcutePostAPI<Survoyer>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                ClaimantId=survoyer.ClaimantId,
                TransactionType=(int)Enums.ClaimTransactionTypes.Reserve
            };
             setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimTransactions,
                Request = mainSearchMC
            };
            var Reserve = Helpers.ExcutePostAPI<List<ClaimTransactions>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");


            decimal OS = 0,Diff=Convert.ToDecimal( survoyer.EstimatedSPAmount+survoyer.EstimatedLabourAmount);
            foreach (var item in Reserve)
            {
                OS += item.TransactionAmount;
            }
            ClaimTransactions claimTransactions = new ClaimTransactions()
            {
                ClaimantID = survoyer.ClaimantId,
                ClaimId = survoyer.ClaimId,
                CreatedBy = HttpContext.Session.getSessionData<Users>("LoggedUser").UserName,
                isActive = true,
                TransactionAmount =  Diff-OS,
                TransactionDate = DateTime.Now,
                TransactionType = (int)Enums.ClaimTransactionTypes.Reserve,
                Note = survoyer.Note,
            };
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimTransaction,
                Request = claimTransactions
            };
            claimTransactions = Helpers.ExcutePostAPI<ClaimTransactions>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            Helpers.RegisterHistory(_appSettings, survoyer.ClaimId, "Update Claim Reserve to " + Diff, HttpContext.Session.getSessionData<Users>("LoggedUser").UserName);

            return RedirectToAction("SurveyorEntry",new { obj = obj });
        }

        [HttpPost]
        public async void UploadAttachments(int Id, int ClaimId,int ClaimantId, int ModuleId, IList<IFormFile> files)
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

        public IActionResult SurveyorAssign(int ClaimId)
        {
            ViewData["DivName"] = "SurveyorAssign";
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
            return View("_SurveyorAssign", claim.FirstOrDefault());
        }      
        
  

        public IActionResult SurveyorActions(int ClaimId)
        {
            List<Claimants> claimants = new List<Claimants>();
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
            claimants = Helpers.ExcutePostAPI<List<Claimants>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            ViewData["claimants"] = claimants;
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
            return View("_UpdateReserve", claim.FirstOrDefault());
        }


        [HttpPost]
        public void UpdateReserveTransaction(Reserve reserve)
        {
            reserve.Creationdate = DateTime.Now;
            
            ClaimTransactions claimTransactions = new ClaimTransactions()
            {
                ClaimantID = Convert.ToInt32(HttpContext.Request.Form["ClaimantID"]),
                ClaimId = reserve.ClaimId,
                CollectionType = string.Empty,
                Collector = 1,
                Commission = 0,
                Fees = 0,
                isActive = true,
                ParentTransactions = null,
                Payment = string.Empty,
                TransactionAmount = reserve.SparePartCost.Value + reserve.OtherCost.Value + reserve.LaborCost.Value,
                TransactionDate = DateTime.Now,
                TransactionType = (int)Enums.ClaimTransactionTypes.Reserve,
                CreatedBy="Admn",
                Note=reserve.Note
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimTransaction,
                Request = claimTransactions
            };
            claimTransactions = Helpers.ExcutePostAPI<ClaimTransactions>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            reserve.CreatedBy = 1;
            reserve.CreatedByName = "Hamza";
            reserve.TransactionId = claimTransactions.Id;
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertReserve,
                Request = reserve
            };
            reserve = Helpers.ExcutePostAPI<Reserve>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
       

        }

        public IActionResult LoadSurveyorsList(string Id)
        {
            ClaimSearchobj claimSearchobj = Helpers.Deserilize<ClaimSearchobj>(Helpers.Decryption(Id));
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                ClaimId = Convert.ToInt32(claimSearchobj.ClaimId),
                ClaimantId = Convert.ToInt32(claimSearchobj.ClaimantId),

            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadSurveyor,
                Request = mainSearchMC
            };
            var survoyers = Helpers.ExcutePostAPI<List<Survoyer>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            HttpContext.Session.SetSessionData("survoyers", survoyers);
            ViewData["claimSearchobj"] = Id;
            ViewData["survoyers"]= survoyers;
            ViewData["ClaimId"]= claimSearchobj.ClaimId;
            ViewData["ClaimantId"] = claimSearchobj.ClaimantId;
            return View("_SurveyorsList");
        }
            
        public IActionResult LoadSurveyorEntry(string Id)
        {
            ClaimSearchobj claimSearchobj = Helpers.Deserilize<ClaimSearchobj>(Helpers.Decryption(Id));
            List<Survoyer> survoyers = new List<Survoyer>();
            survoyers = HttpContext.Session.getSessionData<List<Survoyer>>("survoyers");
            ViewData["survoyers"] = survoyers;
            Survoyer survoyer = new Survoyer();
            if (survoyers.Count>0 && claimSearchobj.SurvoyerId>0)
            {
                survoyer = survoyers.Where(p=>p.Id== claimSearchobj.SurvoyerId).FirstOrDefault();
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
            if (missingParts.Count>0 && claimSearchobj.SurvoyerId>0)
            {
                survoyer = missingParts.Where(p=>p.SurveyId== claimSearchobj.SurvoyerId).FirstOrDefault();
            }
            else
            {
                survoyer.ClaimantId = claimSearchobj.ClaimantId.Value;
                survoyer.ClaimId = claimSearchobj.ClaimId.Value;
                survoyer.SurveyId= claimSearchobj.SurvoyerId.Value;
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
    }
}
