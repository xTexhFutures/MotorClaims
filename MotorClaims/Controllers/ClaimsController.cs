using CORE.DTOs.APIs.Authenticator;
using CORE.DTOs.APIs.Business;
using CORE.DTOs.APIs.MotorClaim;
using CORE.DTOs.APIs.Process.Payments;
using CORE.DTOs.APIs.Setups.MMP;
using CORE.DTOs.Authentications;
using CORE.DTOs.MotorClaim;
using CORE.DTOs.MotorClaim.Claims;
using CORE.DTOs.MotorClaim.Integrations.APIs;
using CORE.DTOs.MotorClaim.WorkFlow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MotorClaims.Models;
using Newtonsoft.Json;
using System.Reflection;
using System.Security.Claims;

namespace MotorClaims.Controllers
{
    public class ClaimsController : Controller
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string VehicleListCacheKey = "ServicesLink";
        private readonly IMemoryCache _memoryCache;
        public static IWebHostEnvironment _environment;
        CORE.DTOs.APIs.TP_Services.APIsLists query = new CORE.DTOs.APIs.TP_Services.APIsLists();

        public ClaimsController(IOptions<AppSettings> appSettings, IWebHostEnvironment environment, IMemoryCache memoryCache)
        {
            _environment = environment;
            _appSettings = appSettings.Value;
            _memoryCache = memoryCache;
            _memoryCache.TryGetValue(VehicleListCacheKey, out query);
        }
        public IActionResult Index(string obj)
        {
            ClaimSearchobj claimSearchobj = Helpers.Deserilize<ClaimSearchobj>(Helpers.Decryption(obj));
            ClaimSearchResult claimSearchResult = HttpContext.Session.getSessionData<ClaimSearchResult>("SearchResult");
            Production policy =claimSearchResult.Productions.Where(p=>p.policy.Id == claimSearchobj.PolicyId).FirstOrDefault();
            var Vehicle=policy.Vehicles.Where(p=>p.Vehicle.Id== claimSearchobj.VehicleId).FirstOrDefault();
            ViewData["Policy"] = policy;
            ViewData["Vehicle"] = Vehicle;
            return View();
        }

        public IActionResult ClaimEntry(string obj)
        {
            ClaimSearchobj claimSearchobj = Helpers.Deserilize<ClaimSearchobj>(Helpers.Decryption(obj));
            ClaimSearchResult claimSearchResult = HttpContext.Session.getSessionData<ClaimSearchResult>("SearchResult");
            var policy = claimSearchResult.Productions.Where(p => p.policy.Id == claimSearchobj.PolicyId).FirstOrDefault();
            var Vehicle = policy.Vehicles.Where(p => p.Vehicle.Id == claimSearchobj.VehicleId).FirstOrDefault();
            Claims claims = new Claims();
            if (claimSearchobj.ClaimId.HasValue && claimSearchobj.ClaimId.Value>0)
            {
                MainSearchMC mainSearchMC = new MainSearchMC()
                {
                    Id = Convert.ToInt32(claimSearchobj.ClaimId.Value)
                };
                SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaim,
                    Request = mainSearchMC
                };
               var claim = Helpers.ExcutePostAPI<List<Claims>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
                claims = claim.FirstOrDefault();
            }
            ViewData["Policy"] = policy;
            ViewData["Vehicle"] = Vehicle;
            return View(claims);
        } 
        public IActionResult ClaimantsEntry(string obj)
        {
            ClaimSearchobj claimSearchobj = Helpers.Deserilize<ClaimSearchobj>(Helpers.Decryption(obj));
            ClaimSearchResult claimSearchResult = HttpContext.Session.getSessionData<ClaimSearchResult>("SearchResult");
            var policy = claimSearchResult.Productions.Where(p => p.policy.Id == claimSearchobj.PolicyId).FirstOrDefault();
            var Vehicle = policy.Vehicles.Where(p => p.Vehicle.Id == claimSearchobj.VehicleId).FirstOrDefault();
            Claimants claimants = new Claimants();
            claimants.ClaimId = claimSearchobj.ClaimId.Value;
            if (claimSearchobj.ClaimId.HasValue && claimSearchobj.ClaimId.Value > 0)
            {
                MainSearchMC mainSearchMC = new MainSearchMC()
                {
                    ClaimId = Convert.ToInt32(claimSearchobj.ClaimId.Value)
                };
                SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimants,
                    Request = mainSearchMC
                };
                var claim = Helpers.ExcutePostAPI<List<Claimants>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
                claimants =claim.Count>0? claim.FirstOrDefault():claimants;
            }

            //SearchLookUp mainSearchMC = new SearchLookUp()
            //{
            //    MajorCode=SystemEnums.sub
            //};
            //var claim = Helpers.ExcutePostAPI<List<LookupTable>>(mainSearchMC, _appSettings.APIHubPrefix + "api/MotorClaim/Loadlookups");
            ViewData["Policy"] = policy;
            ViewData["Vehicle"] = Vehicle;

            List<DDL> ddl= new List<DDL>();
            SearchLookUp searchLookUp = new SearchLookUp()
            {
                domainForCalim=DomainForCalim.ClaimantType
                
            };
            //ddl = Helpers.ExcutePostAPI<List<DDL>>(searchLookUp, _appSettings.APIHubPrefix + "api/MotorClaim/LoadDomains");
            ViewData["ClaimantType"] = ddl;

            ddl = new List<DDL>();
            searchLookUp = new SearchLookUp()
            {
                domainForCalim = DomainForCalim.CaseResult

            };
            //ddl = Helpers.ExcutePostAPI<List<DDL>>(searchLookUp, _appSettings.APIHubPrefix + "api/MotorClaim/LoadDomains");
            ViewData["ClaimantResult"] = ddl;

            ddl = new List<DDL>();
            searchLookUp = new SearchLookUp()
            {
                domainForCalim = DomainForCalim.ClaimantsLoss

            };
            //ddl = Helpers.ExcutePostAPI<List<DDL>>(searchLookUp, _appSettings.APIHubPrefix + "api/MotorClaim/LoadDomains");
            ViewData["ClaimCause"] = ddl;
            return View(claimants);
        }

        [HttpPost]
        public IActionResult InsertClaim(Claims claim)
        {
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new Random();
            int otpPick = _rdm.Next(_min, _max);
            int PolicyId = Convert.ToInt32(HttpContext.Request.Form["PolicyId"]);
            int VehicleId = Convert.ToInt32(HttpContext.Request.Form["VehicleId"]);
            ClaimSearchResult claimSearchResult = HttpContext.Session.getSessionData<ClaimSearchResult>("SearchResult");
                if (claim.ClaimReportType==(int)Models.Enums.ClaimReportType.Manual)
            {
                int ManualReportType= Convert.ToInt32(HttpContext.Request.Form["ManualReportType"]);
                string ReportNo = HttpContext.Request.Form["ReportNo"];
                if (ManualReportType== (int)Models.Enums.ClaimReportType.Najm)
                {
                    claim.AccidentNo = ReportNo;
                }
                else
                {
                    claim.BasherNo = ReportNo;
                }
            }
            var policy = claimSearchResult.Productions.Where(p => p.policy.Id == PolicyId).FirstOrDefault();
            var Vehicle = policy.Vehicles.Where(p => p.Vehicle.Id == VehicleId).FirstOrDefault();
            claim.ChassisNo = Vehicle.Vehicle.ChassisNo;
            claim.PolicyNo = policy.policy.PolicyNumber;
            claim.Beneficiery = policy.policy.BenefecieryName;
            claim.PolicyId = policy.policy.PolicyId;
            claim.InsuredName = policy.policy.Insured;
            claim.VehicleName = Vehicle.Vehicle.Name;
            claim.BusinessType = policy.policy.BusinessTypeId;
            claim.BusinessType_desc = policy.policy.BusinessType;
            claim.InsuredId = policy.policy.InsuredId;
            claim.Owner =policy.policy.OwnerName;
            claim.BranchId = policy.policy.BranchId;
            claim.Branch = policy.policy.BranchName;

            claim.ClaimNo=string.IsNullOrEmpty(claim.ClaimNo)? "C-" +DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString()+DateTime.Now.Day.ToString()+ otpPick.ToString(): claim.ClaimNo;
            claim.ClaimStatus = (int)Enums.Status.InProgress;
            claim.ClaimUWYear=claim.DateOfLoss.Year;
            claim.FraudIndicator = "Low";
            claim.FraudScore = 0;

            claim.PlateNo = Vehicle.Vehicle.PlateNo;
            claim.PolicyEffectiveDate = policy.policy.PolicyEffectiveDate;
            claim.PolicyExpiryDate = policy.policy.PolicyExpiryDate;
            claim.PolicySI = Vehicle.Vehicle.SumInsured;
            claim.PolicyUWYear=policy.policy.PolicyUWYear;
            claim.CreationDate = DateTime.Now;
            bool CheckUpdate=claim.Id==0;
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaim,
                Request = claim
            };
            claim = Helpers.ExcutePostAPI<Claims>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            ClaimVehicle claimVehicle = new ClaimVehicle()
            {
                ClaimId = claim.Id,
                VehicleId = VehicleId
            };
            if (CheckUpdate)
            {
                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertClaimVehicle,
                    Request = claimVehicle
                };
                var CV = Helpers.ExcutePostAPI<ClaimVehicle>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            }

            ClaimSearchobj claimSearchobj = new ClaimSearchobj()
            {
                PolicyId = PolicyId,
                VehicleId = VehicleId,
                ClaimId= claim.Id,
            };


            SearchingObj searchingObj = new SearchingObj()
            {
                PolicyNo = policy.policy.PolicyNumber

            };
            claimSearchResult = new ClaimSearchResult();
            claimSearchResult = Helpers.ExcutePostAPI<ClaimSearchResult>(searchingObj, _appSettings.APIHubPrefix + "api/MotorClaim/SearchClaimInfo");
            HttpContext.Session.SetSessionData("SearchResult", claimSearchResult);
            return RedirectToAction("ClaimantsEntry",  new { obj = Helpers.Encrypt(JsonConvert.SerializeObject(claimSearchobj)) });
        }


        [HttpPost]
        public IActionResult InsertClaimant(Claimants claimants)
        {

            int PolicyId = Convert.ToInt32(HttpContext.Request.Form["PolicyId"]);
            int VehicleId = Convert.ToInt32(HttpContext.Request.Form["VehicleId"]);
            ClaimSearchResult claimSearchResult = HttpContext.Session.getSessionData<ClaimSearchResult>("SearchResult");
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimants,
                Request = claimants
            };
            claimants = Helpers.ExcutePostAPI<Claimants>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            ClaimSearchobj claimSearchobj = new ClaimSearchobj()
            {
                PolicyId = PolicyId,
                VehicleId = VehicleId
            };
            claimSearchResult = HttpContext.Session.getSessionData<ClaimSearchResult>("SearchResult");
            claimSearchResult = new ClaimSearchResult();
            return RedirectToAction("Index", new { obj = Helpers.Encrypt(JsonConvert.SerializeObject(claimSearchobj)) });
        }

        public IActionResult ClaimantsList(int Id)
        {
            List<Claimants> claimantsList = new List<Claimants>();
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                ClaimId = Id
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimants,
                Request = mainSearchMC
            };
            claimantsList = Helpers.ExcutePostAPI<List<Claimants>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            return View(claimantsList);
        }
    }
}

