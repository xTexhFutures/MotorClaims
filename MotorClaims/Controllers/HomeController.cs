using CORE.DTOs.APIs.MotorClaim;
using CORE.DTOs.Authentications;
using CORE.DTOs.MotorClaim.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MotorClaims.Models;
using System.Diagnostics;
using System.Globalization;

namespace MotorClaims.Controllers
{
    public class HomeController : Controller
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string VehicleListCacheKey = "ServicesLink";
        private readonly IMemoryCache _memoryCache;
        public static IWebHostEnvironment _environment;
        CORE.DTOs.APIs.TP_Services.APIsLists query = new CORE.DTOs.APIs.TP_Services.APIsLists();
        public HomeController(IOptions<AppSettings> appSettings, IWebHostEnvironment environment, IMemoryCache memoryCache)
        {
            _environment = environment;
            _appSettings = appSettings.Value;
            _memoryCache = memoryCache;
            _memoryCache.TryGetValue(VehicleListCacheKey, out query);
        }
        public IActionResult Logout()
        {
            HttpContext.Session.SetSessionData("LoggedUser", null);
            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            List<WorkflowTransaction> workflowTransactions = new List<WorkflowTransaction>();
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Status=(int)Enums.WorkflowStatus.Pending,
                UserName= HttpContext.Session.getSessionData<Users>("LoggedUser").UserName

            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadWorkflowTransactions,
                Request = mainSearchMC
            };
            workflowTransactions = Helpers.ExcutePostAPI<List<WorkflowTransaction>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");


            ViewData["Pending"] = workflowTransactions;
             mainSearchMC = new MainSearchMC()
            {
                UserName = HttpContext.Session.getSessionData<Users>("LoggedUser").UserName
            };
             setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadWorkflowTransactions,
                Request = mainSearchMC
            };
            workflowTransactions = Helpers.ExcutePostAPI<List<WorkflowTransaction>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");


            ViewData["Queue"] = workflowTransactions;

            mainSearchMC = new MainSearchMC()
            {
                UserId = HttpContext.Session.getSessionData<Users>("LoggedUser").Id
            };
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadApprovalsTransactions,
                Request = mainSearchMC
            };
            workflowTransactions = Helpers.ExcutePostAPI<List<WorkflowTransaction>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            ViewData["Approvals"] = workflowTransactions;
            ViewData["State"] = "Pass";
            ViewData["Error"] = string.Empty;

            if (string.IsNullOrEmpty(HttpContext.Session.getSessionData<string>("Lang")))
            {
                CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                HttpContext.Session.SetSessionData("Lang", cultureInfo);
            }
           
            List<Users> users = new List<Users>();
            users = HttpContext.Session.getSessionData<List<Users>>("AllUsers");
            if (users == null )
            {
                users = Helpers.ExcuteGetAPI<List<Users>>(true, _appSettings.APIHubPrefix + "api/Authenticator/AllUsers", "123");
                HttpContext.Session.SetSessionData("AllUsers", users);
            }
            List<Users> workshops = new List<Users>();
            List<Users> Agencies = new List<Users>();
            workshops = HttpContext.Session.getSessionData<List<Users>>("workshops");
            if (workshops == null)
            {
                workshops = Helpers.ExcuteGetAPI<List<Users>>(true, _appSettings.APIHubPrefix + "api/Authenticator/AllWorkshops", "123");
                Agencies.Add(new Users()
                {
                    Id=99,
                    Name="Agency1",
                    UserName="Agency1"
                });
                HttpContext.Session.SetSessionData("workshops", workshops);
                HttpContext.Session.SetSessionData("Agencies", Agencies);
            }
            return View();
        }

        public IActionResult MyRequests()
        {
            return View();
        }
        public IActionResult MyPolicies(string a)
        {
            return View();
        }
        public IActionResult MyComplains()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult ChangeLogin(string id)
        {
            CORE.DTOs.APIs.Authenticator.LoginObj loginObj = new CORE.DTOs.APIs.Authenticator.LoginObj();
            loginObj = Helpers.ExcutePostAPI<CORE.DTOs.APIs.Authenticator.LoginObj>(id, _appSettings.APIHubPrefix + query.services.Where(p => p.Name == "LoadUser").FirstOrDefault().Link);
            HttpContext.Session.SetSessionData("LoggedUser", loginObj.Users);
            return RedirectToAction("Index");
        }
    }
}