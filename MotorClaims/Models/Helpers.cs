﻿using CORE.DTOs.APIs.Authenticator;
using CORE.DTOs.APIs.MotorClaim;
using CORE.DTOs.APIs.TP_Services;
using CORE.DTOs.APIs.TPServices;
using CORE.DTOs.MotorClaim;
using CORE.DTOs.MotorClaim.Claims;
using CORE.Interfaces;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace MotorClaims.Models
{
    public static class Helpers
    {
        private static string[] allFormats ={"yyyy/MM/dd","yyyy/M/d",
            "dd/MM/yyyy","d/M/yyyy",
            "dd/M/yyyy","d/MM/yyyy","yyyy-MM-dd",
            "yyyy-M-d","dd-MM-yyyy","d-M-yyyy",
            "dd-M-yyyy","d-MM-yyyy","yyyy MM dd",
            "yyyy M d","dd MM yyyy","d M yyyy",
            "dd M yyyy","d MM yyyy"};
        private static CultureInfo arCul;
        public static T getSessionData<T>(this ISession session, string key)
        {
            try
            {
                var data = session.GetString(key);
                if (data == null)
                {
                    return default(T);
                }
                return JsonConvert.DeserializeObject<T>(data);
            }
            catch (Exception ex)
            {
                return JsonConvert.DeserializeObject<T>("");
            }

        }
        public static void SetSessionData(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }
        public static T Deserilize<T>(string json)
        {
   
            T? resultValue = JsonConvert.DeserializeObject<T>(json);
            return (T)Convert.ChangeType(resultValue, typeof(T));
        }
        public static T PostAPICall<T>(object request, string APILink)
        {

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var myContent = JsonConvert.SerializeObject(request);
            var webRequest = new HttpRequestMessage(HttpMethod.Post, APILink)
            {
                Content = new StringContent(myContent, Encoding.UTF8, "application/json")
            };
            var resultAll = client.Send(webRequest);
            var headers = resultAll.Headers.GetValues("OAuthToken");
            if (string.IsNullOrEmpty(headers.First<string>()))
            {
                var result = new StreamReader(resultAll.Content.ReadAsStream());
                var Info = result.ReadToEnd();

                return (T)Convert.ChangeType(Info, typeof(T));
            }
            else
            {
                T ReterunObj = Deserilize<T>(Decryption(headers.First<string>()));
                return (T)ReterunObj;
            }



        }
        public static T ExcuteGetAPI<T>(object request, string APILink, string token = null)
        {

            HttpClient client = new HttpClient();
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Add("oAuth", token);
            }
            string SendObj = JsonConvert.SerializeObject(request);
            string MessageUrlParams = "?obj=" + Helpers.Encrypt(SendObj).Replace("+", "-");

            client.BaseAddress = new Uri(APILink);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage responsePost = client.GetAsync(MessageUrlParams).Result;

            return Deserilize<T>(responsePost.Content.ReadAsStringAsync().Result);
        }




        public static string ReplaceEmailTemplate(string Body, string UserName, string Status, string URL, string Draft, string Comment)
        {
            Body = Body.Replace("{USER_NAME}", UserName).Replace("{URL}", URL).Replace("{Status}", Status).Replace("{Draft}", Draft).Replace("{COMMENT}", Comment);
            return Body;
        }
        public static string ReplaceUpdateStatusEmailTemplate(string Body, string PolicyNumber, string RequestStatus, string URL)
        {
            Body = Body.Replace("{PolicyNumber}", PolicyNumber).Replace("{URL}", URL).Replace("{RequestStatus}", RequestStatus);
            return Body;
        }
        public static string ReplacePaymentEmailTemplate(string Body, string SeqmentCode, string NetPremium, string URL)
        {
            Body = Body.Replace("{PolicyNumber}", SeqmentCode).Replace("{GrossAmount}", NetPremium).Replace("{LINK}", URL);
            return Body;
        }
        public static string ReplaceRejectionEmailTemplate(string Body, string SeqmentCode, string Comment, string Category, string URL)
        {
            Body = Body.Replace("{PolicyNumber}", SeqmentCode).Replace("{MainCategory}", Category).Replace("{LINK}", URL).Replace("{Comments}", Comment).Replace("{RequestStatus}", "Rejected");
            return Body;
        }
        
        
        public static DateTime HijriToGreg(string hijri)
        {
            arCul = new CultureInfo("ar-SA");
            if (hijri.Length <= 0)
            {

                return DateTime.Now;
            }
            try
            {
                string dt = hijri.Substring(6, 2) + "/" + hijri.Substring(4, 2) + "/" + hijri.Substring(0, 4);
                DateTime tempDate = DateTime.ParseExact(dt,
                   allFormats, arCul.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);
                return tempDate;

            }
            catch (Exception ex)
            {

                return DateTime.Now;
            }
        }
        public static Dictionary<string, object> GetPropertiesNameOfClass(object pObject)
        {
            Dictionary<string, object> propertyList = new Dictionary<string, object>();
            if (pObject != null)
            {
                foreach (var prop in pObject.GetType().GetProperties())
                {
                    propertyList.Add(prop.Name, prop.GetValue(pObject, null));
                }
            }
            return propertyList;
        }
        public static T ExcutePostAPI<T>(object request, string APILink, string token = null)
        {
            HttpClient client = new HttpClient();
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Add("oAuth", token);
            }
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var obj = JsonConvert.SerializeObject(request);
            var webRequest = new HttpRequestMessage(HttpMethod.Post, APILink)
            {
                Content = new StringContent(obj, Encoding.UTF8, "application/json")
            };
            
            var resultAll = client.Send(webRequest);
            var result = new StreamReader(resultAll.Content.ReadAsStream());
            var Info = result.ReadToEnd();
            return Deserilize<T>(Info);

        }
        public static string GetPropertyDescription(object oSource, string PropertyName)
        {
            AttributeCollection attributes = TypeDescriptor.GetProperties(oSource)[PropertyName].Attributes;
            DescriptionAttribute myAttribute = (DescriptionAttribute)attributes[typeof(DescriptionAttribute)];
            return myAttribute.Description.Replace("Gets or sets ", "");
        }
        public static T ExcuteDeleteAPI<T>(object request, string APILink)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var obj = JsonConvert.SerializeObject(request);
            var webRequest = new HttpRequestMessage(HttpMethod.Delete, APILink)
            {
                Content = new StringContent(obj, Encoding.UTF8, "application/json")
            };
            var resultAll = client.Send(webRequest);
            var result = new StreamReader(resultAll.Content.ReadAsStream());
            var Info = result.ReadToEnd();

            return Deserilize<T>(Info);

        }
        public static string Decryption(string hashed)
        {
            byte[] bytesToBeDecrypted = Convert.FromBase64String(hashed);
            byte[] passwordBytesdecrypt = Encoding.UTF8.GetBytes("XBM##@@2023$$");
            byte[] passwordBytes = Encoding.UTF8.GetBytes("XBM##@@2023$$");

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
            passwordBytesdecrypt = SHA256.Create().ComputeHash(passwordBytesdecrypt);

            byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

            string decryptedResult = Encoding.UTF8.GetString(bytesDecrypted);

            return decryptedResult;
        }
        public static int DecryptionNo(int num)
        {
            int NumResult = DateTime.Now.Year - num;

            return NumResult;
        }
        public static int EncryptNo(int num)
        {
            int NumResult = DateTime.Now.Year + num;

            return NumResult;
        }
        public static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 2, 1, 7, 3, 6, 4, 8, 5 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }
        public static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 2, 1, 7, 3, 6, 4, 8, 5 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }
        public static string Encrypt(string phrase)
        {
            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(phrase);
            byte[] passwordBytes = Encoding.UTF8.GetBytes("XBM##@@2023$$");

            // Hash the password with SHA256
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

            string encryptedResult = Convert.ToBase64String(bytesEncrypted);

            return encryptedResult;
        }
        public static int GetRelation(string relation)
        {
            switch (relation)
            {
                case "Parents/اباء/امهات":
                    return 4;
                case "Others/اخرى":
                    return 5;
                case "Child/ابن":
                    return 3;
                case "Spouse/زوج":
                    return 2;
                case "Self/نفسه":
                    return 1;
                default:
                    return 0;
            }
        }
        public static string ReverseRelation(int relation)
        {
            switch (relation)
            {
                case 4:
                    return "Parents/اباء/امهات";
                case 5:
                    return "Others/اخرى";
                case 3:
                    return "Child/ابن";
                case 2:
                    return "Spouse/زوج";
                case 1:
                    return "Self/نفسه";
                default:
                    return "Others";
            }
        }
        public static int GetGender(string Gender)
        {
            switch (Gender)
            {
                case "Male/ذكر":
                    return 1;
                case "Female/انثى":
                    return 2;
                default:
                    return 0;
            }
        }
        public static string ReverseGender(int Gender)
        {
            switch (Gender)
            {
                case 1:
                    return "Male/ذكر";
                case 2:
                    return "Female/انثى";
                default:
                    return "Others";
            }
        }
        public static int GetMartialStatus(string martialStatus)
        {
            switch (martialStatus)
            {
                case "Single/اعزب":
                    return 1;
                case "Married/متزوج":
                    return 2;
                case "Widow/ارمل":
                    return 5;
                case "Divorced/مطلق":
                    return 4;
                default:
                    return 0;
            }
        }
        public static string ReversMartialStatus(int martialStatus)
        {
            switch (martialStatus)
            {
                case 1:
                    return "Single/اعزب";
                case 2:
                    return "Married/متزوج";
                case 5:
                    return "Widow/ارمل";
                case 4:
                    return "Divorced/مطلق";
                default:
                    return "Others";
            }
        }
        public static string FormatDate(DateTime oDate)
        {
            return oDate.ToString("dd-MM-yyyy h:mm tt");
        }
        public static string FormatLongDate(DateTime oDate)
        {
            return oDate.ToString("dd-MM-yyyy h:mm tt");
        }
        public static byte[] ExporttoExcel<T>(List<T> table, string filename)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using ExcelPackage pack = new ExcelPackage();
            ExcelWorksheet ws = pack.Workbook.Worksheets.Add(filename);
            ws.Cells["A1"].LoadFromCollection(table, true, TableStyles.Light1);
            return pack.GetAsByteArray();
        }
        public static DateTime ConvertDate(string dr)
        {
            DateTime DateOfBirth;

            try
            {
                try
                {
                    DateOfBirth = DateTime.ParseExact(dr, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    try
                    {
                        DateOfBirth = Convert.ToDateTime(dr);
                    }
                    catch (Exception XE)
                    {
                        DateOfBirth = DateTime.ParseExact(dr, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    }

                }



            }
            catch (Exception ex)
            {
                try
                {
                    DateOfBirth = DateTime.ParseExact(dr, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }
                catch (Exception x)
                {
                    try
                    {
                        DateOfBirth = DateTime.ParseExact(dr, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            DateOfBirth = DateTime.ParseExact(dr, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                            //string[] f = dr.Split('/');
                            //string f0 =  f[0];
                            //string f1 =  f[1];
                            //string f2 = f[2].Substring(0, 4);
                            //if (f0.Length==3)
                            //{
                            //    f0 = "0" + f0;
                            //}
                            //if (f1.Length == 3)
                            //{
                            //    f1 = "0" + f1;
                            //}
                            //DateOfBirth =DateTime.ParseExact(f2+"/"+f1+"/"+f0, "yyyy/MM/dd", CultureInfo.InvariantCulture);
                            DateOfBirth = DateTime.Now;
                        }

                    }

                }

            }

            return DateOfBirth;
        }
        public static DateTime? CallDate(string dt)
        {
            if (string.IsNullOrEmpty(dt) || dt.Length < 8)
            {
                return (DateTime?)null;
            }
            int year = Convert.ToInt32(dt.Substring(0, 4));
            int Month = Convert.ToInt32(dt.Substring(4, 2));
            int Day = Convert.ToInt32(dt.Substring(6, 2));

            return new DateTime(year, Month, Day);
        }
        public static string GetLookups(string Key,int Type,List<LookupTable> lookupsTables) 
        { 
            string Result=string.Empty;
            Result = lookupsTables.Where(p => p.Code == Key && p.MajorCode == Type).FirstOrDefault().NameEnglish;
            return Result;
        }
        public static string FormatLongDateNoTime(DateTime? oDate)
        {
            if (oDate.HasValue)
            {
                return oDate.Value.ToString("dd-MM-yyyy");
            }
            else
            {
                return string.Empty;
            }
         
        }
        public static void RegisterHistory(AppSettings appSettings,long ClaimId ,string Reason,string LoggedUser,int ClaimantId)
        {
            ClaimHistory claimHistory = new ClaimHistory()
            {
                ChangeDate = DateTime.Now,
                ClaimId = ClaimId,
                Reason = Reason,
                Status = 1,
                ClaimantId= ClaimantId,
                UserName = LoggedUser
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertClaimHistory,
                Request = claimHistory
            };
            var  claims = Helpers.ExcutePostAPI<ClaimHistory>(setupClaimsRequestcs, appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

        }
        public static void AssignClaim(int UserId,int ClaimId, AppSettings appSettings)
        {
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = ClaimId
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaim,
                Request = mainSearchMC
            };
            var claim = Helpers.ExcutePostAPI<Claims>(setupClaimsRequestcs, appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            //claim.AssignTo = UserId;
             setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaim,
                Request = claim
            };
            var claims = Helpers.ExcutePostAPI<Claims>(setupClaimsRequestcs, appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

        }
        public static void SendSMSTemplate(int TemplateId,Dictionary<string,string> Parameters,string MobileNo, AppSettings _appSettings)
        {        
            SMSTemplates sMSTemplates = new SMSTemplates();

            SMSInput sMSInput = new SMSInput()
           {
               MessageBody = "" ,
               Mobile = MobileNo,
               message = "11",
               TemplateId = TemplateId
               
           };
            sMSTemplates = Helpers.ExcutePostAPI<SMSTemplates>(sMSInput, _appSettings.APIHubPrefix + "api/MotorClaim/GetSMSTemplate");
            string MessageBody = sMSTemplates.ArSMS;

            foreach (var param in Parameters)
            {
                MessageBody=MessageBody.Replace("{"+param.Key+"}", param.Value);
            }
            sMSInput.MessageBody = MessageBody;
            var results = Helpers.ExcutePostAPI<CORE.DTOs.APIs.Unified_Response.Results>(sMSInput, _appSettings.APIHubPrefix + "api/ExternalAPIs/SendSms");
        }

        public static void SendNotificationMail(string Language,AppSettings _appSettings,string Email,string ClaimNo)
        {
            string EmailBody = Language == "AR-JO" ? System.IO.File.ReadAllText(Path.Combine(_appSettings.EmailsFolder, "Notification-AR.html")) : System.IO.File.ReadAllText(Path.Combine(_appSettings.EmailsFolder, "Notification-EN.html"));

            EmailInput emailInput = new EmailInput()
            {
                Body = EmailBody,
                Subject = Language == "AR-JO" ? "تنبيه مطالبة - " + ClaimNo : "Claim Notification - " + ClaimNo,
                ToEmail = Email
            };
            var PostingResult = Helpers.ExcutePostAPI<CORE.DTOs.APIs.Unified_Response.Results>(emailInput, _appSettings.APIHubPrefix + "api/ExternalAPIs/SendEmail");
        }
    }
}
