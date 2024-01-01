using CORE.DTOs.APIs.Business;
using CORE.DTOs.APIs.MotorClaim;
using CORE.DTOs.MotorClaim.WorkFlow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MotorClaims.Models;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Engineering;

namespace MotorClaims.Controllers
{
    public class DocumentsController : Controller
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string VehicleListCacheKey = "ServicesLink";
        private readonly IMemoryCache _memoryCache;
        public static IWebHostEnvironment _environment;
        CORE.DTOs.APIs.TP_Services.APIsLists query = new CORE.DTOs.APIs.TP_Services.APIsLists();
        public DocumentsController(IOptions<AppSettings> appSettings, IWebHostEnvironment environment, IMemoryCache memoryCache)
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
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadDocuments,
                Request = mainSearchMC
            };
            var Documents = Helpers.ExcutePostAPI<List<DocumentInfo>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");
            ViewData["Documents"] = Documents;
            return View();
        }
        public IActionResult UpdateDocument(int Id)
        {
            if (Id > 0)
            {
                List<int> ints = new List<int>();
                MainSearchMC mainSearchMC = new MainSearchMC()
                {
                    Id = Id
                };
                SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.LoadDocuments,
                    Request = mainSearchMC
                };
                var Documents = Helpers.ExcutePostAPI<List<DocumentInfo>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

                string[] ClaimantsSections =!string.IsNullOrEmpty(Documents.FirstOrDefault().ClaimResult)? Documents.FirstOrDefault().ClaimResult.Split(','):new string[0];
                foreach (string claimant in ClaimantsSections)
                {
                    if (!string.IsNullOrEmpty(claimant))
                    ints.Add(int.Parse(claimant.Replace(',',' ')));
                }

                ViewData["ints"] = ints;
                return View(Documents.FirstOrDefault());
            }
            return View(new DocumentInfo());

        }

        [HttpPost]
        public IActionResult UpdateDocument(DocumentInfo documentInfo)
        {
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateDocuments,
                Request = documentInfo
            };
            var documents = Helpers.ExcutePostAPI<DocumentInfo>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult SearchDocuments()
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
                ModuleId = Status
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadDocuments,
                Request = mainSearchMC
            };
            var Documents = Helpers.ExcutePostAPI<List<DocumentInfo>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");
            ViewData["Documents"] = Documents;
            ViewData["Error"] = null;
            ViewData["Filter"] = Status;
            return View("Index");
        }

        public IActionResult DeleteDocument(int Id)
        {
            ViewData["Controller"] = "Documents";
            ViewData["Action"] = "DeleteDocumentPost";

            return View("_DeleteConfirmation", Id);
        }

        [HttpPost]
        public IActionResult DeleteDocumentPost(int Id)
        {
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = Id
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.DeleteDocuments,
                Request = mainSearchMC
            };
            var DocumentInfo = Helpers.ExcutePostAPI<bool>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            return RedirectToAction("Index");
        }
    }
}
