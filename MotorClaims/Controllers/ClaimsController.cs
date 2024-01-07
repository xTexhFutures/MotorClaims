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
using System.Drawing;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;

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
            //Dictionary<string,string> keyValuePairs = new Dictionary<string,string>();
            //keyValuePairs.Add("ClaimNo", claims.ClaimNo);
            //keyValuePairs.Add("PlateNo", claims.PlateNo);
            //Helpers.SendSMSTemplate(2, keyValuePairs, "+966592032990", _appSettings);

          
            ViewData["Policy"] = policy;
            ViewData["Vehicle"] = Vehicle;
            ViewData["Cities"] = HttpContext.Session.getSessionData<List<LookupTable>>("Cities");
            return View(claims);
        }

        public IActionResult ClaimantDetails(string obj)
        {
            ViewData["Error"] = string.Empty;
            ClaimSearchResult claimSearchResult = HttpContext.Session.getSessionData<ClaimSearchResult>("SearchResult");
            Production policy = HttpContext.Session.getSessionData<Production>("policy");
            VehicleInfos Vehicle = HttpContext.Session.getSessionData<VehicleInfos>("Vehicle");
            Claimants claimants = new Claimants();
            claimants = HttpContext.Session.getSessionData<Claimants>("claimants");
            ViewData["ClaimantType"] = query.Where(p => p.MajorCode == (int)SystemEnums.ClaimantType).ToList();
            ViewData["DamageType"] = query.Where(p => p.MajorCode == (int)MotorClaims.Models.Enums.Lookups.DamageType).ToList();
            ViewData["Policy"] = policy;
            ViewData["Vehicle"] = Vehicle;
            ViewData["ClaimantResult"] = query.Where(p => p.MajorCode == (int)SystemEnums.ClaimantResult).ToList();

            return View("_ClaimantDetails", claimants);
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
                claimants = claimant != null && claimant.Count > 0 && claimSearchobj.ClaimantId.HasValue && claimSearchobj.ClaimantId.Value > 0 ? claimant.Where(p => p.Id == claimSearchobj.ClaimantId.Value).FirstOrDefault() : claimants;
            }
            ViewData["ClaimantType"] = query.Where(p => p.MajorCode == (int)SystemEnums.ClaimantType).ToList();
            ViewData["DamageType"] = query.Where(p => p.MajorCode == (int)MotorClaims.Models.Enums.Lookups.DamageType).ToList();
            ViewData["Policy"] = policy;
            ViewData["Vehicle"] = Vehicle;
            ViewData["obj"] = obj;
            claimants.ClaimantStatus = claimants.ClaimantStatus.HasValue ? claimants.ClaimantStatus.Value : (int)Enums.ClaimantStatus.Operation;
            HttpContext.Session.SetSessionData("claimants", claimants);
            HttpContext.Session.SetSessionData("policy", policy);
            HttpContext.Session.SetSessionData("Vehicle", Vehicle);
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
            List<Claims> claims = new List<Claims>();
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
            MainSearchMC main = new MainSearchMC()
            {
                AccidentNo = claim.AccidentNo
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaim,
                Request = main
            };
            var policy = claimSearchResult.Productions.Where(p => p.policy.Id == PolicyId).FirstOrDefault();
            var Vehicle = policy.Vehicles.Where(p => p.Vehicle.Id == VehicleId).FirstOrDefault();
            claims = Helpers.ExcutePostAPI<List<Claims>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            ViewData["Policy"] = policy;
            ViewData["Vehicle"] = Vehicle;
            ViewData["Cities"] = HttpContext.Session.getSessionData<List<LookupTable>>("Cities");

            if (claims.Count > 0)
            {
                ViewData["Error"] = "This Accident No " + claim.AccidentNo + " already exist";
                claim = claims.FirstOrDefault();
                return View("ClaimEntry", claim);
            }


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

            claim.ClaimNo = string.IsNullOrEmpty(claim.ClaimNo) ? "C-111-" + claim.AccidentNo + "-" + otpPick.ToString() : claim.ClaimNo;
            claim.ClaimStatus = (int)Enums.Status.InProgress;
            claim.ClaimUWYear = claim.DateOfLoss.Year;
            claim.FraudIndicator = "Low";
            claim.FraudScore = 0;

            claim.PlateNo = Vehicle.Vehicle.PlateNo;
            claim.PolicyEffectiveDate = policy.policy.PolicyEffectiveDate;
            claim.PolicyExpiryDate = policy.policy.PolicyExpiryDate;
            claim.PolicySI = Convert.ToInt32(Vehicle.Vehicle.SumInsured);
            claim.PolicyUWYear = policy.policy.PolicyUWYear;
            claim.CreationDate = DateTime.Now;
            bool CheckUpdate = claim.Id == 0;
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaim,
                Request = claim
            };
            claim = Helpers.ExcutePostAPI<Claims>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            ClaimVehicle claimVehicle = new ClaimVehicle()
            {
                ClaimId = claim.Id,
                RiskId = VehicleId,
                PolicyId = PolicyId,
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
                PolicyNo = policy.policy.PolicyNumber,
                SequenceNo = Vehicle.Vehicle.SequanceNo,
                ClaimNo = claim.ClaimNo

            };
            claimSearchResult = new ClaimSearchResult();
            claimSearchResult = Helpers.ExcutePostAPI<ClaimSearchResult>(searchingObj, _appSettings.APIHubPrefix + "api/MotorClaim/SearchClaimInfo");
            HttpContext.Session.SetSessionData("SearchResult", claimSearchResult);



            return View("ClaimEntry", claim);
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

            AutoAssignObj autoAssignObj = new AutoAssignObj()
            {
                ClaimantId = claimants.Id,
                RoleId = (int)Models.Enums.ClaimantStatus.Operation,
                Status =  2
            };
            claimants = Helpers.ExcutePostAPI<Claimants>(autoAssignObj, _appSettings.APIHubPrefix + "api/MotorClaim/AutoAssign");


            return RedirectToAction("ClaimEntry", new { obj = Helpers.Encrypt(JsonConvert.SerializeObject(claimSearchobj)) });
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
            ViewData["query"] = query;
            return View(claimantsList);
        }

        public IActionResult DocumentsUpload(int ModuleId, int PolicyId, int VehicleId, int? ClaimId = null, int? ClaimantId = null, string? Reference = null)
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
               Id=(int)ClaimId,
               ClaimantId=ClaimantId
            };
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
                Request = mainSearchMC
            };
            var claims = Helpers.ExcutePostAPI<List<ClaimMaster>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            ViewData["Link"] = _appSettings.DocumentsLink +"/"+ claims.FirstOrDefault().claims.ClaimNo+"/"+ claims.FirstOrDefault().claimants.Serial+"/";
            return View(claims.FirstOrDefault());
        }

        [HttpPost]
        public async Task<IActionResult> UploadAttachments(int Id, int ClaimId, int ModuleId, int? ClaimantId,int attachId,int serial, IList<IFormFile> files)
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

            List<Claims> claims = new List<Claims>();
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
                    CreatedBy = HttpContext.Session.getSessionData<Users>("LoggedUser").UserName,
                    IsDeleted = false,
                    Id= attachId
                };
                SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateAttachment,
                    Request = attachment
                };
                attachment = Helpers.ExcutePostAPI<Attachments>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

                foreach (IFormFile file in files)
                {

                    string pathMDF = _appSettings.ClaimSubmissionPath;
                    string fieNameWithExt = attachment.Id.ToString() + "_" + Path.GetFileName(file.FileName);
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    if (file.Length > 0)
                    {
                        SetupClaimsRequestcs mainSearch = new SetupClaimsRequestcs()
                        {
                            TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaim,
                            Request = new MainSearchMC()
                            {
                                Id =(int)attachment.ClaimId
                            }
                        };
                        claims = Helpers.ExcutePostAPI<List<Claims>>(mainSearch, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

                        string directory = Path.Combine(pathMDF, claims.FirstOrDefault().ClaimNo,serial.ToString());
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
                claims.DateOfLoss = !string.IsNullOrEmpty(najmResponse.najmAccidentinfo.callDate) ? Helpers.CallDate(najmResponse.najmAccidentinfo.callDate).Value : claims.DateOfLoss;
                claims.AccidentPlace = najmResponse.najmAccidentinfo.AccidentDescription;
                claims.RegistrationDate = DateTime.Now;
                claims.AccidentNo = Id;
                claims.TaqdeerNo = Taqdeer;
                var City = query.Where(p => p.MajorCode == (int)SystemEnums.City && p.NameEnglish.ToUpper().Trim().Replace(" ", "").Contains(najmResponse.najmAccidentinfo.city.ToUpper().Trim().Replace(" ", ""))).FirstOrDefault();
                if (City != null)
                {
                    claims.City = City.NameEnglish;
                    claims.CityId = Convert.ToInt32(City.Code);
                }

                taqdeerResponse = Helpers.ExcutePostAPI<TaqdeerResponse>(Taqdeer, _appSettings.APIHubPrefix + "api/MotorClaim/TaqdeerDetails");
                if (!string.IsNullOrEmpty(taqdeerResponse.TaqdeerCase.DACaseNumber))
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


        [HttpPost]
        public Claims BasherDetails(string Id, string SequanceNo, string Taqdeer)
        {
            ViewData["Error"] = string.Empty;
            BasherResponseCode basherResponse = new BasherResponseCode();
            TaqdeerResponse taqdeerResponse = new TaqdeerResponse();
            Claims claims = new Claims();
            BasherFilter basherFilter = new BasherFilter()
            {
                accidentnumber = Id,
                vehiclesequence = SequanceNo
            };
            basherResponse = Helpers.ExcutePostAPI<BasherResponseCode>(basherFilter, _appSettings.APIHubPrefix + "api/MotorClaim/GetBasherInfo");
            if (basherResponse != null && basherResponse.basherAccidentinfo.accidentNumber == 0)
            {
                claims.ClaimStatus = 0;
                claims.Notes = "There are no info for this Claim " + Id.ToUpper();
            }
            else
            {
                claims.ClaimStatus = 1;
                claims.DateOfLoss = !string.IsNullOrEmpty(basherResponse.basherAccidentinfo.accidentDate) ? Helpers.HijriToGreg(basherResponse.basherAccidentinfo.accidentDate) : claims.DateOfLoss;
                claims.AccidentPlace = basherResponse.basherAccidentinfo.accidentDescription;
                claims.RegistrationDate = DateTime.Now;
                claims.AccidentNo = Id;
                claims.TaqdeerNo = Taqdeer;
                var City = query.Where(p => p.MajorCode == (int)SystemEnums.City && p.NameEnglish.ToUpper().Trim().Replace(" ", "").Contains(basherResponse.basherAccidentinfo.accidentCity.ToUpper().Trim().Replace(" ", ""))).FirstOrDefault();
                if (City != null)
                {
                    claims.City = City.NameEnglish;
                    claims.CityId = Convert.ToInt32(City.Code);
                }

                if (!string.IsNullOrEmpty(Taqdeer))
                {
                    taqdeerResponse = Helpers.ExcutePostAPI<TaqdeerResponse>(Taqdeer, _appSettings.APIHubPrefix + "api/MotorClaim/TaqdeerDetails");
                    if (!string.IsNullOrEmpty(taqdeerResponse.TaqdeerCase.DACaseNumber))
                    {
                        claims.TaqdeerNo = Taqdeer.ToUpper();
                    }
                    else
                    {
                        claims.ClaimStatus = 0;
                        claims.Notes = "There are no Taqdeer info for this Claim " + Taqdeer.ToUpper();
                    }
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
                var claim = Helpers.ExcutePostAPI<List<Claims>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
                claims = claim.FirstOrDefault();
            }
            ViewData["ClaimantType"] = query.Where(p => p.MajorCode == (int)SystemEnums.ClaimantType).ToList();
            ViewData["ClaimantResult"] = query.Where(p => p.MajorCode == (int)Enums.Lookups.DamageType).ToList();
            ViewData["Policy"] = policy;
            ViewData["Vehicle"] = Vehicle;
            ViewData["claims"] = claims;
            return View(claimants);
        }

        public IActionResult ClaimantReserve(int? ClaimantId, int? ClaimId)
        {
            ViewData["ClaimantId"] = ClaimantId;
            ViewData["ClaimId"] = ClaimId;
            List<ClaimTransactions> claimTransactions = new List<ClaimTransactions>();
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                TransactionType = (int)Enums.ClaimTransactionTypes.Reserve,
                ClaimantId = ClaimantId
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimTransactions,
                Request = mainSearchMC
            };
            claimTransactions = Helpers.ExcutePostAPI<List<ClaimTransactions>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            return View("_ClaimantReserve", claimTransactions);
        }
        public IActionResult ClaimantRecovery(int? ClaimantId, int? ClaimId)
        {
            ViewData["ClaimantId"] = ClaimantId;
            ViewData["ClaimId"] = ClaimId;
            List<ClaimTransactions> claimTransactions = new List<ClaimTransactions>();
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                TransactionType = (int)Enums.ClaimTransactionTypes.Recovery,
                ClaimantId = ClaimantId
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimTransactions,
                Request = mainSearchMC
            };
            claimTransactions = Helpers.ExcutePostAPI<List<ClaimTransactions>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            return PartialView("_ClaimantRecovery", claimTransactions);
        }


        public IActionResult UpdateClaimantReserve(ClaimTransactions claimTransaction)
        {
            claimTransaction.TransactionDate = DateTime.Now;
            claimTransaction.TransactionType = 1;
            claimTransaction.CreatedBy = "Admin";
            claimTransaction.isActive = true;

            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimTransaction,
                Request = claimTransaction
            };
            claimTransaction = Helpers.ExcutePostAPI<ClaimTransactions>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");



            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                TransactionType = 1,
                ClaimantId = claimTransaction.ClaimantID
            };
            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimTransactions,
                Request = mainSearchMC
            };
            var claimTransactions = Helpers.ExcutePostAPI<List<ClaimTransactions>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            ViewData["ClaimantId"] = claimTransaction.ClaimantID;
            ViewData["ClaimId"] = claimTransaction.ClaimId;

            return PartialView("_ClaimantReserve", claimTransactions);
        }

        public IActionResult Photos(int Id)
        {

            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = Convert.ToInt32(Id)
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaim,
                Request = mainSearchMC
            };
            var claim = Helpers.ExcutePostAPI<List<Claims>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            DirectoryInfo d = new DirectoryInfo(string.Concat(_appSettings.NajmImagesPath, "\\LD\\", claim.FirstOrDefault().AccidentNo)); //Assuming Test is your Folder
            List<string> location = new List<string>();
            ViewData["Claims"] = claim.FirstOrDefault();
            try
            {
                FileInfo[] Files = d.GetFiles("*.jpg"); //Getting Text files

                string str = "";

                foreach (FileInfo file in Files)
                {
                    location.Add(file.Name);
                }
                ViewData["location"] = location;


                d = new DirectoryInfo(string.Concat(_appSettings.NajmImagesPath, "\\DA\\", claim.FirstOrDefault().TaqdeerNo)); //Assuming Test is your Folder

                Files = d.GetFiles("*.jpg"); //Getting Text files
                location = new List<string>();

                foreach (FileInfo file in Files)
                {
                    location.Add(file.Name);
                }
                ViewData["location1"] = location;
            }
            catch (Exception)
            {
            }
         

            return PartialView("_Photos");
        }

        public IActionResult ClaimsHistory(string SequanceNo)
        {

            var claim = Helpers.ExcutePostAPI<List<Claims>>(SequanceNo, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimHistoryCount");
            return PartialView("_ClaimsCount", claim);
        }
    }
}

