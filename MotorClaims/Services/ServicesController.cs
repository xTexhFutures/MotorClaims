using CORE.DTOs.Authentications;
using CORE.DTOs.MotorClaim.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MotorClaims.Models;

namespace MotorClaims.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string LookupTable = "LookupTable";
        private readonly IMemoryCache _memoryCache;
        public static IWebHostEnvironment _environment;
        List<LookupTable> query = new List<LookupTable>();
        public ServicesController(IOptions<AppSettings> appSettings, IWebHostEnvironment environment, IMemoryCache memoryCache)
        {
            _environment = environment;
            _appSettings = appSettings.Value;
            _memoryCache = memoryCache;
            _memoryCache.TryGetValue(LookupTable, out query);
        }
        [Route("GetUsers")]
        [HttpGet]
        public List<Users> GetUsers(string name)
        {

            List<Users> users = new List<Users>();
            users = Helpers.ExcuteGetAPI<List<Users>>(true, _appSettings.APIHubPrefix + "api/Authenticator/AllUsers", "123");       
            return users;
        }
        [Route("GetLookup")]
        [HttpGet]
        public List<LookupTable> GetLookup(string name, int enums,int? ParentId=null)
        {
            return query.Where(p=>p.NameEnglish.ToUpper().Contains(name.ToUpper()) && p.MajorCode==enums && (ParentId.HasValue && ParentId.Value>0?p.ParentId==ParentId.Value:p.Id==p.Id)).Take(10).ToList();
        }
    }
}
