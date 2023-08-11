using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MotorClaims.Models;

namespace MotorClaims.Controllers
{
    public class DelegationController : Controller
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string VehicleListCacheKey = "ServicesLink";
        private readonly IMemoryCache _memoryCache;
        public static IWebHostEnvironment _environment;
        CORE.DTOs.APIs.TP_Services.APIsLists query = new CORE.DTOs.APIs.TP_Services.APIsLists();
        public DelegationController(IOptions<AppSettings> appSettings, IWebHostEnvironment environment, IMemoryCache memoryCache)
        {
            _environment = environment;
            _appSettings = appSettings.Value;
            _memoryCache = memoryCache;
            _memoryCache.TryGetValue(VehicleListCacheKey, out query);
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UpdateDelegation(int Id)
        {
            return View(Id);
        }

        [HttpPost]
        public IActionResult UpdateDelegation()
        {
            string DelegationFrom= HttpContext.Request.Form["DelegateFrom"];
            int hfDelegationFrom= Convert.ToInt32(HttpContext.Request.Form["hfDelegateFrom"]);
            string DelegationTo= HttpContext.Request.Form["DelegateTo"];
            int hfDelegationTo = Convert.ToInt32(HttpContext.Request.Form["hfDelegateTo"]);
            DateTime From=Helpers.ConvertDate( HttpContext.Request.Form["From"]);
            DateTime To = Helpers.ConvertDate(HttpContext.Request.Form["To"]);
            int Id = 0;
            try
            {
                Id = Convert.ToInt32(HttpContext.Request.Form["Id"]);
            }
            catch (Exception)
            {
            }
            return View("UpdateDelegation");
        }
    }
}
