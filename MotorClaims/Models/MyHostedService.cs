using CORE.DTOs.MotorClaim.Claims;
using CORE.DTOs.MotorClaim.Integrations.APIs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Linq;

namespace MotorClaims.Models
{
    public class MyHostedService : IHostedService
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string lookupCacheKey = "LookupTable";
        private readonly IMemoryCache _memoryCache;


        public MyHostedService(IOptions<AppSettings> appSettings, IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _appSettings = appSettings.Value;


        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {

            List<LookupTable> lookupTables = new List<LookupTable>();
            SearchLookUp searchLookUp1 = new SearchLookUp();
            lookupTables = Helpers.ExcutePostAPI<List<LookupTable>>(searchLookUp1, _appSettings.APIHubPrefix + "api/MotorClaim/Loadlookups");


            if (lookupTables!=null && lookupTables.Count>0)
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromDays(1))
                .SetAbsoluteExpiration(TimeSpan.FromDays(1));

                _memoryCache.Set(lookupCacheKey, lookupTables, cacheOptions);
            }

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _memoryCache.Remove(lookupCacheKey);
        }
    }
}
