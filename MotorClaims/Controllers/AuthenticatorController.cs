﻿using CORE.DTOs.APIs.Authenticator;
using CORE.DTOs.APIs.TP_Services;
using CORE.DTOs.Authentications;
using CORE.DTOs.Authentications.APIs;
using CORE.DTOs.MotorClaim.Claims;
using CORE.DTOs.MotorClaim.Integrations.APIs;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MotorClaims.Models;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;

namespace MotorClaims.Controllers
{
    public class AuthenticatorController : Controller
    {
        private static HttpClient client = new HttpClient();
        private readonly AppSettings _appSettings;
        private const string LookupTable = "LookupTable";
        private readonly IMemoryCache _memoryCache;
        public static IWebHostEnvironment _environment;
        List<LookupTable> query = new List<LookupTable>();
        public AuthenticatorController(IOptions<AppSettings> appSettings, IWebHostEnvironment environment, IMemoryCache memoryCache)
        {
            _environment = environment;
            _appSettings = appSettings.Value;
            _memoryCache = memoryCache;
            _memoryCache.TryGetValue(LookupTable, out query);
        }
        public IActionResult AccessDeny()
        {
            string Error = "You don't have permission to do this action";
            return PartialView("AccessDeny", Error);
        }



        public ActionResult Login(string? confirm = null)
        {

            ViewData["State"] = "Pass";
            ViewData["confirm"] = confirm;
            if (!string.IsNullOrEmpty(confirm))
            {
                ViewData["Error"] = confirm;
            }
            try
            {
                if (HttpContext.Session.getSessionData<Users>("LoggedUser") != null)
                    return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
            }


            return View("Authentication/_Login");
        }

        [HttpPost]
        public ActionResult Verify(string Id)
        {
            ViewData["State"] = "Pass";
            return View("Authentication/_Verify", Id);
        }
        public ActionResult ForgetPassword()
        {
            ViewData["State"] = "Pass";
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(string Id)
        {
            ViewData["State"] = "Pass";
            return PartialView("Authentication/_ChangePassword", Id);
        }

        [HttpPost]
        public IActionResult Resend()
        {
            ViewData["State"] = "Pass";
            ViewData["Error"] = string.Empty;




            Users usr = new Users();
            usr = HttpContext.Session.getSessionData<Users>("user");

            if (usr == null)
            {

            }

            int _min = 1000;
            int _max = 9999;
            string error = string.Empty;
            Random _rdm = new Random();
            int otpPick = _rdm.Next(_min, _max);

            HttpContext.Session.SetSessionData("OTP", otpPick);

            CORE.DTOs.APIs.TPServices.SMSInput sMS = new CORE.DTOs.APIs.TPServices.SMSInput()
            {
                MessageBody = "Login password is : " + otpPick.ToString(),
                Mobile = usr.Mobile,
                message = "11"
            };
            CORE.DTOs.APIs.Unified_Response.Results results = new CORE.DTOs.APIs.Unified_Response.Results();
            //results = Helpers.ExcutePostAPI<CORE.DTOs.APIs.Unified_Response.Results>(sMS,_appSettings.APIHubPrefix+ query.services.Where(p => p.Name == "SMS").FirstOrDefault().Link);


            return Verify(usr.Mobile);
        }


        [HttpPost]
        public ActionResult VerifyOTP()
        {
            ViewData["State"] = "Pass";
            ViewData["Error"] = string.Empty;

            Users usr = new Users();
            usr = HttpContext.Session.getSessionData<Users>("user");
            LoginObj loginObj = new LoginObj();
            loginObj = HttpContext.Session.getSessionData<LoginObj>("loginObj");
            if (loginObj.Users.FailedAttempt == 3 || !loginObj.Users.IsActive)
            {
                Users users = new Users();
                users = loginObj.Users;
                InsertUser insertUser = new InsertUser()
                {
                    Email = users.Email,
                    EskaId = Convert.ToInt32(users.EskaId),
                    Mobile = Convert.ToInt32(users.Mobile),
                    Password = users.Password,
                    Username = users.UserName,
                    status = false,
                    TeamLeader = users.ManagerId

                };
                loginObj = Helpers.ExcutePostAPI<CORE.DTOs.APIs.Authenticator.LoginObj>(insertUser, _appSettings.APIHubPrefix + "api/Authenticator/UpdateUser");
                ViewData["State"] = "Error";
                ViewData["Error"] = "Your account is In-Active";
                return Verify(usr.Mobile);
            }

            try
            {
                string message = string.Empty;
                string OTP1 = HttpContext.Request.Form["digit-1"].ToString();
                string OTP2 = HttpContext.Request.Form["digit-2"].ToString();
                string OTP3 = HttpContext.Request.Form["digit-3"].ToString();
                string OTP4 = HttpContext.Request.Form["digit-4"].ToString();
                string OTP = string.Concat(OTP1, OTP2, OTP3, OTP4);

                if (OTP == HttpContext.Session.getSessionData<string>("OTP"))
                {
                    List<LookupTable> lookupTables = new List<LookupTable>();
                    lookupTables = query.Where(p => p.MajorCode == (int)Enums.Lookups.City).ToList();
                    if (lookupTables == null)
                    {
                        SearchLookUp searchLookUp = new SearchLookUp()
                        {
                            MajorCode = SystemEnums.City
                        };
                        lookupTables = Helpers.ExcutePostAPI<List<LookupTable>>(searchLookUp, _appSettings.APIHubPrefix + "api/MotorClaim/Loadlookups");
                    }

                    HttpContext.Session.SetSessionData("Cities", lookupTables);
                    loginObj.Employees.Add(loginObj.Users);
                    HttpContext.Session.SetSessionData("OrginalUser", usr);
                    HttpContext.Session.SetSessionData("LoggedUser", usr);
                    HttpContext.Session.SetSessionData("TeamMembers", loginObj.Employees);
                    HttpContext.Session.SetSessionData("UserName", usr.UserName);
                    CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                    HttpContext.Session.SetSessionData("Lang", cultureInfo);

                    if ((int)Enums.Roles.Sales == loginObj.Roles?.Id || (int)Enums.Roles.Agent == loginObj.Roles?.Id || (int)Enums.Roles.Broker == loginObj.Roles?.Id || (int)Enums.Roles.Admin == loginObj.Roles?.Id)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    if ((int)Enums.Roles.UWApproval == loginObj.Roles?.Id)
                    {
                        return RedirectToAction("UWApprovals", "Approvals");
                    }
                    if ((int)Enums.Roles.Approval == loginObj.Roles?.Id)
                    {
                        return RedirectToAction("Approvals", "Approvals");
                    }
                    if ((int)Enums.Roles.Finance == loginObj.Roles?.Id)
                    {
                        return RedirectToAction("FinanceApprovals", "Approvals");
                    }
                    if ((int)Enums.Roles.IT == loginObj.Roles?.Id)
                    {
                        return RedirectToAction("Index", "Managing");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }

                }
                else if (OTP == HttpContext.Session.getSessionData<string>("ResetPassword"))
                {
                    string error = string.Empty;
                    usr.IsOneTimePassword = false;

                    return ChangePassword(usr.Email);
                }
                else
                {
                    Users users = new Users();
                    users = loginObj.Users;
                    users.FailedAttempt = users.FailedAttempt.HasValue ? ++users.FailedAttempt : 1;
                    InsertUser insertUser = new InsertUser()
                    {
                        Email = users.Email,
                        EskaId = Convert.ToInt32(users.EskaId),
                        Mobile = Convert.ToInt32(users.Mobile),
                        Password = users.Password,
                        Username = users.UserName,
                        status = users.IsActive,
                        FailledAttemp = users.FailedAttempt.Value

                    };
                    var y = Helpers.ExcutePostAPI<CORE.DTOs.APIs.Authenticator.LoginObj>(insertUser, _appSettings.APIHubPrefix + "api/Authenticator/UpdateUser");
                    loginObj.Users = users;
                    HttpContext.Session.SetSessionData("loginObj", loginObj);
                    ViewData["State"] = "Error";
                    ViewData["Error"] = "OTP Is not correct";
                    return Verify(usr.Mobile);
                }



            }
            catch
            {
                return Verify(usr.Mobile);
            }
        }

        [HttpPost]
        public IActionResult LoginUser()
        {

            ViewData["State"] = "Pass";
            ViewData["Error"] = string.Empty;
            InsuranceAPIs.Models.Authentications.Login login = new InsuranceAPIs.Models.Authentications.Login()
            {
                Password = Helpers.Encrypt(HttpContext.Request.Form["Password"]),
                UserName = HttpContext.Request.Form["UserName"]
            };

            LoginObj loginObj = new LoginObj();

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var myContent = JsonConvert.SerializeObject(login);
            var webRequest = new HttpRequestMessage(HttpMethod.Post, _appSettings.APIHubPrefix + "api/Authenticator/Login")
            {
                Content = new StringContent(myContent, Encoding.UTF8, "application/json")
            };
            var resultAll = client.Send(webRequest);
            try
            {
                var headers = resultAll.Headers.GetValues("OAuthToken");
                if (string.IsNullOrEmpty(headers.First<string>()))
                {
                    var result = new StreamReader(resultAll.Content.ReadAsStream());
                    var Info = result.ReadToEnd();
                    ViewData["State"] = "Error";
                    ViewData["Error"] = Info;
                    return RedirectToAction("Login",new { confirm=Info });
                }
                else
                {
                    loginObj = Helpers.Deserilize<CORE.DTOs.APIs.Authenticator.LoginObj>(Helpers.Decryption(headers.First<string>()));


                    HttpContext.Session.SetSessionData("loginObj", loginObj);
                }
            }
            catch (Exception)
            {

                ViewData["State"] = "Error";
                ViewData["Error"] = "Wrong UserName or Password";
                return View("Authentication/_Login");

            }

            if (loginObj != null && loginObj.Users != null && loginObj.Users.Id > 0)
            {
                if (!loginObj.Users.IsActive)
                {
                    ViewData["State"] = "Error";
                    ViewData["Error"] = "InActive User";
                    return View("Authentication/_Login");

                }
                //string result = System.IO.Path.GetTempPath();
                //try
                //{
                // Check if file already exists. If yes, delete it.     
                //if (loginObj.Users != null && !string.IsNullOrEmpty(loginObj.Users.AccessKey))
                //{
                //    string Mask = string.Empty;
                //    IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
                //    NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                //    foreach (NetworkInterface adapter in nics)
                //    {
                //        IPInterfaceProperties properties = adapter.GetIPProperties();
                //        PhysicalAddress address = adapter.GetPhysicalAddress();
                //        byte[] bytes = address.GetAddressBytes();
                //        for (int i = 0; i < bytes.Length; i++)
                //        {
                //            Mask = Mask + bytes[i].ToString("X2");
                //            if (i != bytes.Length - 1)
                //            {
                //                Mask = Mask + ("-");
                //            }
                //        }
                //    }
                //if(Mask != loginObj.Users.AccessKey)
                //{
                //    ViewData["State"] = "Error";
                //    ViewData["Error"] = "Un Authorized to access this User.";
                //    return View("Authentication/_Login");
                //}
                //}

                //}
                //catch (Exception Ex)
                //{
                //    Console.WriteLine(Ex.ToString());
                //}


                HttpContext.Session.SetSessionData("user", loginObj.Users);
                if (loginObj.Users.IsOneTimePassword)
                {
                    return RedirectToAction("ForgetPassword", "Authenticator");
                }
                int _min = 1000;
                int _max = 9999;
                loginObj.Users.FailedAttempt = 0;
                //_svcUserManagments.InsertUpdateUser(usr, out error);
                Random _rdm = new Random();
                int otpPick = _rdm.Next(_min, _max);


                otpPick = 1111;
                HttpContext.Session.SetSessionData("OTP", otpPick);


                CORE.DTOs.APIs.TPServices.SMSInput sMS = new CORE.DTOs.APIs.TPServices.SMSInput()
                {
                    MessageBody = "Login password is : " + otpPick.ToString(),
                    Mobile = loginObj.Users.Mobile,
                    message = "11"
                };
                CORE.DTOs.APIs.Unified_Response.Results results = new CORE.DTOs.APIs.Unified_Response.Results();
                //results = Helpers.ExcutePostAPI<CORE.DTOs.APIs.Unified_Response.Results>(sMS, _appSettings.APIHubPrefix + "api/ExternalAPIs/SendSms");

                return View("Authentication/_Verify", loginObj.Users.Mobile);

            }
            else
            {
                ViewData["State"] = "Error";
                ViewData["Error"] = "Invalid Email";
                return View("Authentication/_Login");
            }


        }


        [HttpPost]
        public IActionResult UpdatePassword()
        {
            string Password = Helpers.Encrypt(HttpContext.Request.Form["Password"]);
            string ConfirmPassword = Helpers.Encrypt(HttpContext.Request.Form["ConPassword"]);
            string Email = HttpContext.Request.Form["Email"];
            CORE.DTOs.APIs.Authenticator.LoginObj loginObj = new CORE.DTOs.APIs.Authenticator.LoginObj();
            loginObj = Helpers.ExcutePostAPI<CORE.DTOs.APIs.Authenticator.LoginObj>(Email, _appSettings.APIHubPrefix + "api/Authenticator/LoadUserByEmail");

            string error = string.Empty;

            if (Password != ConfirmPassword)
            {
                ViewData["State"] = "Error";
                ViewData["Error"] = "Password & Confirm Password not matched !!";
                return PartialView("Authentication/_ChangePassword", null);
            }
            else
            {
                Users usr = new Users();
                usr = loginObj.Users;
                UpdatePassword updatePassword = new UpdatePassword()
                {
                    Password = Password,
                    Username = usr.UserName
                };
                UserInfoAPI userInfoAPI = new UserInfoAPI();
                userInfoAPI = Helpers.ExcutePostAPI<UserInfoAPI>(updatePassword, _appSettings.APIHubPrefix + "api/Authenticator/UpdatePassword");
                return RedirectToAction("Login");
            }

        }


        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnURL)
        {
            HttpContext.Session.SetSessionData("Lang", culture);
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }

            );
            return LocalRedirect(returnURL);
        }

        public IActionResult UnAuthorized()
        {
            return View();
        }


        //public IActionResult ResetPassword()
        //{
        //    ViewData["State"] = "Pass";
        //    return View();
        //}

        //[HttpGet]
        //public IActionResult ResetPassword(string token, string email)
        //{
        //    var model = new ResetPasswordModel { Token = token, Email = email };
        //    ViewData["State"] = "Pass";
        //    return View("Authentication/_ChangePassword", model.Email);
        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ForgotPassword(ForgotPasswordModel forgotPasswordModel)
        //{
        //    if (!ModelState.IsValid)
        //        return View(forgotPasswordModel);

        //    CORE.DTOs.APIs.Authenticator.LoginObj loginObj = new CORE.DTOs.APIs.Authenticator.LoginObj();
        //    loginObj = Helpers.ExcutePostAPI<CORE.DTOs.APIs.Authenticator.LoginObj>(forgotPasswordModel.Email, _appSettings.APIHubPrefix + "api/Authenticator/LoadUserByEmail");

        //    if (loginObj == null || loginObj.Users == null)
        //        return RedirectToAction(nameof(ForgetPassword));
        //    Guid guid = Guid.NewGuid();
        //    var token = guid.ToString();
        //    var callback = Url.Action(nameof(ResetPassword), "Authenticator", new { token, email = forgotPasswordModel.Email }, Request.Scheme);
        //    EmailInput emailInput = new EmailInput()
        //    {
        //        ToEmail = forgotPasswordModel.Email,
        //        isApproval = true,
        //        Subject = "Email Confirmation",
        //        Body = callback
        //    };
        //    var resultNew = Helpers.ExcutePostAPI<CORE.DTOs.APIs.Unified_Response.Results>(emailInput, _appSettings.APIHubPrefix + query.services.Where(p => p.Name == "SendEmail").FirstOrDefault().Link);

        //    return RedirectToAction("Login", new { confirm = "Confirm" });
        //}
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Forget()
        {
            string UserName = HttpContext.Request.Form["Email"];

            CORE.DTOs.APIs.Authenticator.LoginObj loginObj = new CORE.DTOs.APIs.Authenticator.LoginObj();
            loginObj = Helpers.ExcutePostAPI<CORE.DTOs.APIs.Authenticator.LoginObj>(UserName, _appSettings.APIHubPrefix + "api/Authenticator/LoadUser");

            if (loginObj != null && loginObj.Users != null && loginObj.Users.Id > 0)
            {

                HttpContext.Session.SetSessionData("user", loginObj.Users);
                int _min = 1000;
                int _max = 9999;
                string error = string.Empty;
                Random _rdm = new Random();
                int otpPick = _rdm.Next(_min, _max);

                //otpPick = 1111;

                HttpContext.Session.SetSessionData("ResetPassword", otpPick);
                CORE.DTOs.APIs.Unified_Response.Results results = new CORE.DTOs.APIs.Unified_Response.Results();
                CORE.DTOs.APIs.TPServices.SMSInput sMS = new CORE.DTOs.APIs.TPServices.SMSInput()
                {
                    MessageBody = "Reset password OTP is : " + otpPick.ToString(),
                    Mobile = loginObj.Users.Mobile,
                    message = "11"
                };
                results = Helpers.ExcutePostAPI<CORE.DTOs.APIs.Unified_Response.Results>(sMS, _appSettings.APIHubPrefix + "api/Authenticator/SMS");
                //sMSResponseModel = _svcSMS.SendSMS(_appSettings, "Reset password OTP is : " + otpPick, usr.Mobile);
                // _svcSMS.InsertSMS(sMSResponseModel, out error);

                return View("Authentication/_Verify", loginObj.Users.Mobile);

            }

            ViewData["State"] = "Pass";
            ViewData["Error"] = "OTP was sent";
            return View("Authentication/_Verify", "");



        }

    }
}
