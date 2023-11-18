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
using CORE.DTOs.Setups;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MotorClaims.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Security.Claims;

namespace MotorClaims.Controllers
{
    public class ClaimsController : Controller
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string LookupTable = "LookupTable";
        private readonly IMemoryCache _memoryCache;
        public static IWebHostEnvironment _environment;
        List<LookupTable> query = new List<LookupTable>();

        public ClaimsController(IOptions<AppSettings> appSettings, IWebHostEnvironment environment, IMemoryCache memoryCache)
        {
            _environment = environment;
            _appSettings = appSettings.Value;
            _memoryCache = memoryCache;
            _memoryCache.TryGetValue(LookupTable, out query);
        }
        public IActionResult Index(string obj)
        {
            ClaimSearchobj claimSearchobj = Helpers.Deserilize<ClaimSearchobj>(Helpers.Decryption(obj));
            ClaimSearchResult claimSearchResult = HttpContext.Session.getSessionData<ClaimSearchResult>("SearchResult");
            Production policy = claimSearchResult.Productions.Where(p => p.policy.Id == claimSearchobj.PolicyId).FirstOrDefault();
            var Vehicle = policy?.Vehicles.Where(p => p.Vehicle.Id == claimSearchobj.VehicleId).FirstOrDefault();
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
            if (claimSearchobj.ClaimId.HasValue && claimSearchobj.ClaimId.Value > 0)
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


            List<LookupTable> lookupTables = new List<LookupTable>();
            lookupTables = query.Where(p=>p.MajorCode==(int)Enums.Lookups.City).ToList();
            if (lookupTables == null)
            {
                SearchLookUp searchLookUp = new SearchLookUp()
                {
                    MajorCode = SystemEnums.City
                };
                lookupTables = Helpers.ExcutePostAPI<List<LookupTable>>(searchLookUp, _appSettings.APIHubPrefix + "api/MotorClaim/Loadlookups");
            }

            HttpContext.Session.SetSessionData("Cities", lookupTables);
            ViewData["Policy"] = policy;
            ViewData["Vehicle"] = Vehicle;
            ViewData["Cities"] = lookupTables;
            return View(claims);
        }
        public IActionResult ClaimantsEntry(string obj)
        {
            ViewData["Error"] = string.Empty;
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
                var claimant = Helpers.ExcutePostAPI<List<Claimants>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
                claimants = claimant.Count > 0 && claimSearchobj.ClaimantId.HasValue && claimSearchobj .ClaimantId.Value>0? claimant.Where(p=>p.Id== claimSearchobj.ClaimantId.Value).FirstOrDefault() : claimants;
            }        
            ViewData["ClaimantType"] = query.Where(p=>p.MajorCode==(int)SystemEnums.ClaimantType).ToList();
            ViewData["DamageType"] = query.Where(p=>p.MajorCode==(int)MotorClaims.Models.Enums.Lookups.DamageType).ToList();
            ViewData["Policy"] = policy;
            ViewData["Vehicle"] = Vehicle;
            ViewData["ClaimantResult"] = query.Where(p => p.MajorCode == (int)SystemEnums.ClaimantResult).ToList();

            return View("_ClaimantsEntry", claimants);
        }

        [HttpPost]
        public IActionResult InsertClaim(Claims claim)
        {
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new Random();
            int otpPick = _rdm.Next(_min, _max);
            int PolicyId = Convert.ToInt32(HttpContext.Request.Form["PolicyId"].ToString().Split(",")[0]);
            int VehicleId = Convert.ToInt32(HttpContext.Request.Form["VehicleId"].ToString().Split(",")[0]);
            ClaimSearchResult claimSearchResult = HttpContext.Session.getSessionData<ClaimSearchResult>("SearchResult");
            if (claim.ClaimReportType == (int)Models.Enums.ClaimReportType.Manual)
            {
                int ManualReportType = Convert.ToInt32(HttpContext.Request.Form["ManualReportType"]);
                string ReportNo = HttpContext.Request.Form["ReportNo"];
                if (ManualReportType == (int)Models.Enums.ClaimReportType.Najm)
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
            claim.InsuredId = policy.policy.InsuredId.ToString();
            claim.Owner = policy.policy.OwnerName;
            claim.BranchId = policy.policy.BranchId;
            claim.Branch = policy.policy.BranchName;

            claim.ClaimNo = string.IsNullOrEmpty(claim.ClaimNo) ? "C-" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + otpPick.ToString() : claim.ClaimNo;
            claim.ClaimStatus = (int)Enums.Status.InProgress;
            claim.ClaimUWYear = claim.DateOfLoss.Year;
            claim.FraudIndicator = "Low";
            claim.FraudScore = 0;

            claim.PlateNo = Vehicle.Vehicle.PlateNo;
            claim.PolicyEffectiveDate = policy.policy.PolicyEffectiveDate;
            claim.PolicyExpiryDate = policy.policy.PolicyExpiryDate;
            claim.PolicySI =Convert.ToInt32( Vehicle.Vehicle.SumInsured);
            claim.PolicyUWYear = policy.policy.PolicyUWYear;
            claim.CreationDate = DateTime.Now;
            bool CheckUpdate = claim.Id == 0;
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaim,
                Request = claim
            };
            claim = Helpers.ExcutePostAPI<Claims>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            ClaimVehicle claimVehicle = new ClaimVehicle()
            {
                ClaimId = claim.Id,
                RiskId = VehicleId,
                PolicyId= PolicyId,
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
                ClaimId = claim.Id,
            };


            SearchingObj searchingObj = new SearchingObj()
            {
                PolicyNo = policy.policy.PolicyNumber

            };
            claimSearchResult = new ClaimSearchResult();
            claimSearchResult = Helpers.ExcutePostAPI<ClaimSearchResult>(searchingObj, _appSettings.APIHubPrefix + "api/MotorClaim/SearchClaimInfo");
            HttpContext.Session.SetSessionData("SearchResult", claimSearchResult);
            return RedirectToAction("ClaimantsEntry", new { obj = Helpers.Encrypt(JsonConvert.SerializeObject(claimSearchobj)) });
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
                VehicleId = VehicleId,
                ClaimId = claimants.ClaimId
            };
            claimSearchResult = HttpContext.Session.getSessionData<ClaimSearchResult>("SearchResult");
            claimSearchResult = new ClaimSearchResult();
            return RedirectToAction("ClaimantsEntry", new { obj = Helpers.Encrypt(JsonConvert.SerializeObject(claimSearchobj)) });
        }

        public IActionResult ClaimantsList(int Id, int PolicyId, int VehicleId)
        {
            ViewData["VehicleId"] = VehicleId;
            ViewData["PolicyId"] = PolicyId;
            List<Claimants> claimantsList = new List<Claimants>();
            Claims claims = new Claims();
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

            mainSearchMC = new MainSearchMC()
            {
                Id = Convert.ToInt32(Id)
            };
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaim,
                Request = mainSearchMC
            };
            var claim = Helpers.ExcutePostAPI<List<Claims>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            claims = claim.FirstOrDefault();
            ViewData["claims"] = claims;
            return View(claimantsList);
        }

        public IActionResult DocumentsUpload(int ModuleId, int PolicyId, int VehicleId, int? ClaimId = null, int? ClaimantId = null,string? Reference=null)       
        {
            ViewData["VehicleId"] = VehicleId;
            ViewData["PolicyId"] = PolicyId;
            ViewData["ClaimId"] = ClaimId;
            ViewData["ClaimantId"] = ClaimantId;
            ViewData["ModuleId"] = ModuleId;
            List<DocumentInfo> documentInfos = new List<DocumentInfo>();
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                ModuleId = ModuleId
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadDocuments,
                Request = mainSearchMC
            };
            documentInfos = Helpers.ExcutePostAPI<List<DocumentInfo>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            ViewData["documentInfos"] = documentInfos;
            List<Attachments> attachments = new List<Attachments>();
            mainSearchMC = new MainSearchMC()
            {
                ModuleId = ModuleId
            };
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadAttachment,
                Request = mainSearchMC
            };
            attachments = Helpers.ExcutePostAPI<List<Attachments>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");
            ViewData["Link"] = _appSettings.DocumentsLink + Reference;
            return View(attachments);
        }

        [HttpPost]
        public async Task<IActionResult> UploadAttachments(int Id, int ClaimId, int ModuleId,int? ClaimantId, IList<IFormFile> files)
        {
            int VehicleId = Convert.ToInt32(HttpContext.Request.Form["VehicleId"]);
            int PolicyId = Convert.ToInt32(HttpContext.Request.Form["PolicyId"]);
            //int? ClaimId = Convert.ToInt32(HttpContext.Request.Form["ClaimId"]);
            //int? ClaimantId = Convert.ToInt32(HttpContext.Request.Form["ClaimantId"]);
            //int ModuleId = Convert.ToInt32(HttpContext.Request.Form["ModuleId"]);
            ViewData["VehicleId"] = VehicleId;
            ViewData["PolicyId"] = PolicyId;
            ViewData["ClaimId"] = ClaimId;
            ViewData["ClaimantId"] = ClaimantId;
            ViewData["ModuleId"] = ModuleId;

            List < Claims> claims = new List<Claims>();
            List<Attachments> attachments = new List<Attachments>();
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
                    IsDeleted=false
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
                        SetupClaimsRequestcs mainSearch = new SetupClaimsRequestcs()
                        {
                            TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaim,
                            Request=new MainSearchMC()
                            {
                                Id=attachment.ClaimId
                            }
                        };
                        claims = Helpers.ExcutePostAPI< List<Claims>>(mainSearch, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

                        string directory = Path.Combine(pathMDF, claims.FirstOrDefault().ClaimNo);
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
                ViewData["Link"] = _appSettings.DocumentsLink + claims.FirstOrDefault().ClaimNo;
                List<DocumentInfo> documentInfos = new List<DocumentInfo>();
                MainSearchMC mainSearchMC = new MainSearchMC()
                {
                    ModuleId = ModuleId
                };
                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.LoadDocuments,
                    Request = mainSearchMC
                };
                documentInfos = Helpers.ExcutePostAPI<List<DocumentInfo>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

                ViewData["documentInfos"] = documentInfos;

                mainSearchMC = new MainSearchMC()
                {
                    ModuleId = ModuleId
                };
                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.LoadAttachment,
                    Request = mainSearchMC
                };
                attachments = Helpers.ExcutePostAPI<List<Attachments>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");
            }

            return View("DocumentsUpload", attachments);
        }

        [HttpPost]
        public Claims NajmDetails(string Id, string Taqdeer)
        {
            ViewData["Error"] = string.Empty;
            NajmResponse najmResponse = new NajmResponse();
            TaqdeerResponse taqdeerResponse = new TaqdeerResponse();
            Claims claims = new Claims();
            najmResponse = Helpers.ExcutePostAPI<NajmResponse>(Id, _appSettings.APIHubPrefix + "api/MotorClaim/NajmDetails");
            if (najmResponse.najmAccidentinfo.Id == 0)
            {
                claims.ClaimStatus = 0;
                claims.Notes = "There are no Najm info for this Claim " + Id.ToUpper();
            }
            else
            {
                claims.ClaimStatus = 1;
                claims.DateOfLoss =!string.IsNullOrEmpty( najmResponse.najmAccidentinfo.callDate) ?Helpers.ConvertDate( najmResponse.najmAccidentinfo.callDate) : claims.DateOfLoss;
                claims.AccidentPlace = najmResponse.najmAccidentinfo.landmark;
                claims.RegistrationDate = DateTime.Now;
                claims.AccidentNo = Id;
                claims.TaqdeerNo = Taqdeer;
                var City = query.Where(p => p.MajorCode == (int)SystemEnums.City && p.NameEnglish.ToUpper().Trim().Replace(" ", "").Contains(najmResponse.najmAccidentinfo.city.ToUpper().Trim().Replace(" ", ""))).FirstOrDefault();
                if (City != null)
                {
                    claims.City = City.NameEnglish;
                    claims.CityId =Convert.ToInt32( City.Code);
                }
                
                taqdeerResponse = Helpers.ExcutePostAPI<TaqdeerResponse>(Taqdeer, _appSettings.APIHubPrefix + "api/MotorClaim/TaqdeerDetails");
                if (!string.IsNullOrEmpty( taqdeerResponse.TaqdeerCase.DACaseNumber) )
                {
                    claims.TaqdeerNo = Taqdeer.ToUpper();
                }
                else
                {
                    claims.ClaimStatus = 0;
                    claims.Notes = "There are no Taqdeer info for this Claim " + Taqdeer.ToUpper();
                }
            }
            claims.AccidentNo = Id;
            return claims;
        }

        public IActionResult ClaimManagment(string obj)
        {
            ClaimSearchobj claimSearchobj = Helpers.Deserilize<ClaimSearchobj>(Helpers.Decryption(obj));
            ClaimSearchResult claimSearchResult = HttpContext.Session.getSessionData<ClaimSearchResult>("SearchResult");
            var policy = claimSearchResult.Productions.Where(p => p.policy.Id == claimSearchobj.PolicyId).FirstOrDefault();
            var Vehicle = policy.Vehicles.Where(p => p.Vehicle.Id == claimSearchobj.VehicleId).FirstOrDefault();

            List<Claimants> claimants = new List<Claimants>();
            Claims claims = new Claims();
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
                claimants = Helpers.ExcutePostAPI<List<Claimants>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

                mainSearchMC = new MainSearchMC()
                {
                    Id = Convert.ToInt32(claimSearchobj.ClaimId.Value)
                };
                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaim,
                    Request = mainSearchMC
                };
               var  claim = Helpers.ExcutePostAPI<List<Claims>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
                claims=claim.FirstOrDefault();
            }
            ViewData["ClaimantType"] = query.Where(p => p.MajorCode == (int)SystemEnums.ClaimantType).ToList();
            ViewData["ClaimantResult"] = query.Where(p => p.MajorCode == (int)SystemEnums.ClaimantResult).ToList();
            ViewData["Policy"] = policy;
            ViewData["Vehicle"] = Vehicle;
            ViewData["claims"] = claims;
            return View(claimants);
        }
    }
}

