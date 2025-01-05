using CORE.DTOs.APIs.Business;
using CORE.DTOs.APIs;
using CORE.DTOs.APIs.MotorClaim;
using CORE.DTOs.APIs.Process.Reports;
using CORE.DTOs.MotorClaim.Claims;
using CORE.DTOs.MotorClaim.WorkFlow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MotorClaims.Models;

namespace MotorClaims.Controllers
{
    public class SetupController : Controller
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string VehicleListCacheKey = "ServicesLink";
        private readonly IMemoryCache _memoryCache;
        public static IWebHostEnvironment _environment;
        CORE.DTOs.APIs.TP_Services.APIsLists query = new CORE.DTOs.APIs.TP_Services.APIsLists();
        public SetupController(IOptions<AppSettings> appSettings, IWebHostEnvironment environment, IMemoryCache memoryCache)
        {
            _environment = environment;
            _appSettings = appSettings.Value;
            _memoryCache = memoryCache;
            _memoryCache.TryGetValue(VehicleListCacheKey, out query);
        }
        [CustomAuthorize(Roles = "Admin")]     
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Authority(string err = null, int? Status = null)
        {

                ViewData["Error"] = err;
                ViewData["Filter"] = Status;
                MainSearchMC mainSearchMC = new MainSearchMC();
                SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.LoadAuthorityMatrix,
                    Request = mainSearchMC
                };
                var AuthorityMatrix = Helpers.ExcutePostAPI<List<AuthorityMatrix>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");
                ViewData["AuthorityMatrix"] = AuthorityMatrix;
                return View();
        }


        public IActionResult UpdateAuthority(int Id)
        {
            if (Id > 0)
            {
                MainSearchMC mainSearchMC = new MainSearchMC()
                {
                    Id = Id
                };
                SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.LoadAuthorityMatrix,
                    Request = mainSearchMC
                };
                var Documents = Helpers.ExcutePostAPI<List<AuthorityMatrix>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

                return View(Documents.FirstOrDefault());
            }
            return View(new AuthorityMatrix());

        }

        [HttpPost]
        public IActionResult UpdateAuthority(AuthorityMatrix Authority)
        {
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateDocuments,
                Request = Authority
            };
            var documents = Helpers.ExcutePostAPI<AuthorityMatrix>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            return RedirectToAction("Authority");
        }

        [HttpPost]
        public IActionResult SearchAuthorities()
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
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadAuthorityMatrix,
                Request = mainSearchMC
            };
            var Documents = Helpers.ExcutePostAPI<List<AuthorityMatrix>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");
            ViewData["AuthorityMatrix"] = Documents;
            ViewData["Error"] = null;
            ViewData["Filter"] = Status;
            return View("Authority");
        }


        public IActionResult Reports()
        {
            return View();
        }

        [HttpPost]
        public string PrintReports(int Id)
        {
            //int Id = Convert.ToInt32(HttpContext.Request.Form["Id"]);
            //int PolicyId = Convert.ToInt32(HttpContext.Request.Form["PolicyId"]);
          
            PrintReportInput printReportInput = new PrintReportInput();
            ReportOut reportOut = new ReportOut();
            printReportInput = new PrintReportInput()
            {
                ReportCode = "MCC001",
                Parameters = Id.ToString(),
                ReportOutputType = 1
            };
            string[] p = null;
            try
            {
                reportOut = Helpers.ExcuteGetAPI<ReportOut>(printReportInput, _appSettings.APIHubPrefix + "api/Process/PrintReport");
                p = reportOut.Path.Split('\\');
            }
            catch (Exception)
            {
            }
            //var result = Helpers.ExcutePostAPI<CORE.DTOs.APIs.Unified_Response.Results>(reportInput, _appSettings.APIHubPrefix + query.services.Where(p => p.Name == "GenerateCoreReport").FirstOrDefault().Link);
            string i = reportOut.Path.Replace("E:\\MCReportsPortal\\Files\\ReportsStoragePath", "https://najmclaim.aicc.com.sa:4436/").Replace("\\", "/");
            //byte[] content =await GetUrlContent(i);
            return i;

            
        }
    }
}
