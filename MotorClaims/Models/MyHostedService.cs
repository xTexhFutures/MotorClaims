using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace MotorClaims.Models
{
    public class MyHostedService : IHostedService
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string VehicleListCacheKey = "ServicesLink";
        private readonly IMemoryCache _memoryCache;


        public MyHostedService(IOptions<AppSettings> appSettings, IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _appSettings = appSettings.Value;


        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {

            //List<CORE.DTOs.APIs.TP_Services.APIsLists> services = new List<CORE.DTOs.APIs.TP_Services.APIsLists>();
            //HttpResponseMessage response = await client.GetAsync(_appSettings.APIHubPrefix + _appSettings.APIHubURL);

            //if (response.IsSuccessStatusCode)
            //{
            //    dynamic t = response.Content.ReadAsStringAsync().Result;
            //    var response1 = JsonConvert.DeserializeObject<CORE.DTOs.APIs.TP_Services.APIsLists>(t);

            //    var cacheOptions = new MemoryCacheEntryOptions()
            //    .SetSlidingExpiration(TimeSpan.FromDays(1))
            //    .SetAbsoluteExpiration(TimeSpan.FromDays(1));

            //    CORE.DTOs.APIs.TP_Services.APIsLists query = response1;
            //    _memoryCache.Set(VehicleListCacheKey, query, cacheOptions);
            //}

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _memoryCache.Remove(VehicleListCacheKey);
        }
    }
}
