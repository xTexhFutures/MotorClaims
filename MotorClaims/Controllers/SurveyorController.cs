using CORE.DTOs.APIs.Business;
using CORE.DTOs.APIs.MotorClaim;
using CORE.DTOs.APIs.TP_Services;
using CORE.DTOs.MotorClaim;
using CORE.DTOs.MotorClaim.Claims;
using CORE.DTOs.MotorClaim.Productions;
using CORE.DTOs.MotorClaim.Survoyer;
using CORE.DTOs.MotorClaim.WorkFlow;
using CORE.DTOs.Setups;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MotorClaims.Models;
using System.Security.Claims;

namespace MotorClaims.Controllers
{
    public class SurveyorController : Controller
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string VehicleListCacheKey = "ServicesLink";
        private readonly IMemoryCache _memoryCache;
        public static IWebHostEnvironment _environment;
        CORE.DTOs.APIs.TP_Services.APIsLists query = new CORE.DTOs.APIs.TP_Services.APIsLists();

        public SurveyorController(IOptions<AppSettings> appSettings, IWebHostEnvironment environment, IMemoryCache memoryCache)
        {
            _environment = environment;
            _appSettings = appSettings.Value;
            _memoryCache = memoryCache;
            _memoryCache.TryGetValue(VehicleListCacheKey, out query);
        }

        public IActionResult Index()
        {
            return View(new List<ClaimMaster>());
        }

        public IActionResult SurveyorEntry(string obj)
        {
            ClaimSearchobj claimSearchobj = Helpers.Deserilize<ClaimSearchobj>(Helpers.Decryption(obj));
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                ClaimId = Convert.ToInt32(claimSearchobj.ClaimId.Value)
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimVehicle,
                Request = mainSearchMC
            };
            var claim = Helpers.ExcutePostAPI<ClaimVehicle>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            mainSearchMC = new MainSearchMC()
            {
                PolicyId = Convert.ToInt32(claimSearchobj.PolicyId)
            };
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadProductionInfo,
                Request = mainSearchMC
            };
            var policy = Helpers.ExcutePostAPI<List<ProductionInfo>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            mainSearchMC = new MainSearchMC()
            {
                Id = Convert.ToInt32(claim.RiskId)
            };
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadVehicleInfo,
                Request = mainSearchMC
            };
            var Vehicle = Helpers.ExcutePostAPI<List<VehiclesInfo>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            mainSearchMC = new MainSearchMC()
            {
                Id = Convert.ToInt32(claim.Id)
            };
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaim,
                Request = mainSearchMC
            };
            var ClaimInfo = Helpers.ExcutePostAPI<List<Claims>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");


            Production production = new Production();
            production.policy = policy.FirstOrDefault();

            VehicleInfos vehicleInfos = new VehicleInfos();
            vehicleInfos.Vehicle = Vehicle.FirstOrDefault();
            vehicleInfos.lsClaims = ClaimInfo;
            ViewData["Policy"] = production;
            ViewData["Vehicle"] = vehicleInfos;

            Survoyer survoyer = new Survoyer()
            {
                ClaimId = claim.ClaimId
            };
            return View(survoyer);
        }

        [HttpPost]
        public IActionResult SearchSurveyors()
        {
            MainSearchMC mainSearchMC = new MainSearchMC();
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
                Request = mainSearchMC
            };
            var claim = Helpers.ExcutePostAPI<List<ClaimMaster>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            return View("Index", claim);
        }

        [HttpPost]
        public IActionResult InsertSurveyor(Survoyer survoyer)
        {

            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateSurvoyerEntry,
                Request = survoyer
            };
            survoyer = Helpers.ExcutePostAPI<Survoyer>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async void UploadAttachments(int Id, int ClaimId, int ModuleId, IList<IFormFile> files)
        {

            if (files != null && files.Count > 0)
            {
                Attachments attachment = new Attachments()
                {
                    ClaimantId = (int?)null,
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
                        string directory = Path.Combine(pathMDF, attachment.ClaimId.ToString());
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
                TransactionType = (int)Enums.ClaimTransactionTypes.Reserve
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
    }
}
