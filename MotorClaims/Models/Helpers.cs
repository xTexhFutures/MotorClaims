using CORE.DTOs.APIs.Authenticator;
using CORE.DTOs.APIs.MotorClaim;
using CORE.DTOs.APIs.Process;
using CORE.DTOs.APIs.TP_Services;
using CORE.DTOs.APIs.TPServices;
using CORE.DTOs.Authentications;
using CORE.DTOs.MotorClaim;
using CORE.DTOs.MotorClaim.Claims;
using CORE.DTOs.MotorClaim.WorkFlow;
using CORE.DTOs.Setups;
using CORE.Interfaces;
using CORE.Services;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Claims;
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
        public static string GetLookups(string Key, int Type, List<LookupTable> lookupsTables)
        {
            string Result = string.Empty;
            if (!string.IsNullOrEmpty(Key))
            {
                Result = lookupsTables.Where(p => p.Code == Key && p.MajorCode == Type).FirstOrDefault().NameEnglish;
            }

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
        public static void RegisterHistory(AppSettings appSettings, long ClaimId, string Reason, string LoggedUser, int ClaimantId)
        {
            ClaimHistory claimHistory = new ClaimHistory()
            {
                ChangeDate = DateTime.Now,
                ClaimId = ClaimId,
                Reason = Reason,
                Status = 1,
                ClaimantId = ClaimantId,
                UserName = LoggedUser
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertClaimHistory,
                Request = claimHistory
            };
            var claims = Helpers.ExcutePostAPI<ClaimHistory>(setupClaimsRequestcs, appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

        }
        public static void AssignClaim(int UserId, int ClaimId, AppSettings appSettings)
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
        public static void SendSMSTemplate(int TemplateId, Dictionary<string, string> Parameters, string MobileNo, AppSettings _appSettings)
        {
            SMSTemplates sMSTemplates = new SMSTemplates();

            SMSInput sMSInput = new SMSInput()
            {
                MessageBody = "",
                Mobile = MobileNo,
                message = "11",
                TemplateId = TemplateId

            };
            sMSTemplates = Helpers.ExcutePostAPI<SMSTemplates>(sMSInput, _appSettings.APIHubPrefix + "api/MotorClaim/GetSMSTemplate");
            string MessageBody = sMSTemplates.ArSMS;

            foreach (var param in Parameters)
            {
                MessageBody = MessageBody.Replace("{" + param.Key + "}", param.Value);
            }
            sMSInput.MessageBody = MessageBody;
            var results = Helpers.ExcutePostAPI<CORE.DTOs.APIs.Unified_Response.Results>(sMSInput, _appSettings.APIHubPrefix + "api/ExternalAPIs/SendSms");
        }
        public static void SendSms(string MobileNo, string text, AppSettings _appSettings)
        {

            SMSInput sMSInput = new SMSInput()
            {
                MessageBody = text,
                Mobile = MobileNo,
                message = "11"

            };
            var results = Helpers.ExcutePostAPI<CORE.DTOs.APIs.Unified_Response.Results>(sMSInput, _appSettings.APIHubPrefix + "api/ExternalAPIs/SendSms");
        }
        public static void SendNotificationMail(string Language, AppSettings _appSettings, string Email, string ClaimNo)
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

        public static string GetRecoveryType(int RecoveryTypeId)
        {
            switch (RecoveryTypeId)
            {
                case 1: return "Insurance Company";
                case 2: return "Individual";
                case 3: return "Selling Scrap";
                case 4: return "Deductible";
                case 5: return "Depreciation";
                case 10: return "Recovery From Insured";
                case 11: return "deductible + depreciation";
                case 12: return "Debitor";
                case 14: return "SAMA new TP policy";
                default: return null;


            }
        }

        public static string GetRecoveryReason(int RecoveryReasonId)
        {
            switch (RecoveryReasonId)
            {
                case 1: return "Use of Vehicle usage is different from policy schedule.";
                case 2: return "Exceeding the number of passenger capacity";
                case 3: return "Driven against the direction of traffic";
                case 4: return "Driven under the influence of drugs.";
                case 5: return "Driven by a person under the age 18 year.";
                case 6: return "Driven does not hold a proper class of License.";
                case 7: return "License was expired.";
                case 8: return "Driver escaped the scene of the accident.";
                case 9: return "Running a red light";
                case 10: return "Submitting inaccurate information in proposal form.";
                case 11: return "Proved that the accident was deliberate.";
                case 12: return "Failure to notify within 20 working days of any material.";
                case 13: return "Vehicle was stolen or taken forcibly";
                case 14: return "Not Applicable";
                default: return null;



            }
        }

        public static int GetWorkflowLimit(decimal Limit)
        {
            if (Limit >= -600000 && Limit <= 30000)
            {
                return 30000;
            }
            if (Limit > 30000 && Limit <= 100000)
            {
                return 100000;
            }
            if (Limit > 100000 && Limit <= 150000)
            {
                return 150000;
            }
            if (Limit > 150000 && Limit <= 2000000)
            {
                return 2000000;
            }
            return 6000000;
        }

        public static bool CheckPendingApprovals(int ClaimantId,int WorkflowHeaderId, AppSettings _appSettings)
        {
            bool Status = false;

            List<WorkflowTransaction> workflowTransactions = new List<WorkflowTransaction>();
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                ClaimantId = ClaimantId

            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadWorkflowTransactions,
                Request = mainSearchMC
            };
            workflowTransactions = Helpers.ExcutePostAPI<List<WorkflowTransaction>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            workflowTransactions = workflowTransactions.Where(p => p.Status == (int)Enums.WorkflowStatus.Pending && (WorkflowHeaderId==0?1==1:p.WorkflowHeaderId==WorkflowHeaderId)).ToList();
            if (workflowTransactions.Count > 0)
                Status = true;

            return Status;
        }
        public static void PublishWorkflow(decimal Limit, Enums.WorkflowType WorkflowType, int? ClaimTransactionId, long ClaimId, int ClaimantId, string CreatedBy, List<Users> users, AppSettings _appSettings)
        {
            int ApprovalsLimit = GetWorkflowLimit(Limit);
            List<Users> users1 = new List<Users>();
            WorkflowTransaction workflowTransaction;
            WorkflowTransactionApprovers workflowTransactionApprovers;


            if (WorkflowType==Enums.WorkflowType.Reserve)
            {
                users1 = users.Where(p => Convert.ToInt32(p.EstimateAuthority) == ApprovalsLimit && p.EstimateAuthority > 0).ToList();
            }
            else if (WorkflowType == Enums.WorkflowType.SettelmentAutherity)
            {
                users1 = users.Where(p => Convert.ToInt32(p.ClaimApprovalAuthority) == ApprovalsLimit && p.ClaimApprovalAuthority > 0).ToList();
            }


            if (users1.Count > 0)
            {
                MainSearchMC mainSearchMC = new MainSearchMC()
                {
                    Id = (int)ClaimId,
                    ClaimantId = ClaimantId
                };
                SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
                    Request = mainSearchMC
                };
                var claims = Helpers.ExcutePostAPI<List<ClaimMaster>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");


                workflowTransaction = new WorkflowTransaction()
                {
                    ClaimantId = ClaimantId,
                    ClaimId = ClaimId,
                    ClaimTransactionId = ClaimTransactionId,
                    Status = (int)Enums.WorkflowStatus.Pending,
                    CreationDate = DateTime.Now,
                    CreatedBy = CreatedBy,
                    ClaimNo = claims.FirstOrDefault().claims.ClaimNo + "/" + claims.FirstOrDefault().claimants.Serial,
                    WorkflowHeaderId = (int)WorkflowType
                };
                 setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertWorkflowTransaction,
                    Request = workflowTransaction
                 };
                workflowTransaction = Helpers.ExcutePostAPI<WorkflowTransaction>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
                int ApproverLevel = 1;decimal? LevelAmt=0;
                if (WorkflowType==Enums.WorkflowType.Reserve)
                {
                    users1 = users1.OrderBy(p => p.EstimateAuthority).ToList();
                }
                else if (WorkflowType == Enums.WorkflowType.SettelmentAutherity)
                {
                    users1 = users1.OrderBy(p => p.ClaimApprovalAuthority).ToList();
                }
                else if (WorkflowType == Enums.WorkflowType.TotalLoss)
                {
                    users1 = users1.OrderBy(p => p.RTLAuthority).ToList();
                }
                foreach (Users user in users1)
                {
                    if (WorkflowType == Enums.WorkflowType.Reserve)
                    {
                        ApproverLevel = LevelAmt== user.EstimateAuthority? ApproverLevel : ++ApproverLevel;
                        LevelAmt = LevelAmt== user.EstimateAuthority? LevelAmt: user.EstimateAuthority;
                    }
                    else if (WorkflowType == Enums.WorkflowType.SettelmentAutherity)
                    {
                        ApproverLevel = LevelAmt == user.ClaimApprovalAuthority ? ApproverLevel : ++ApproverLevel;
                        LevelAmt = LevelAmt == user.ClaimApprovalAuthority ? LevelAmt : user.ClaimApprovalAuthority;
                    }
                    else if (WorkflowType == Enums.WorkflowType.TotalLoss)
                    {
                        ApproverLevel = LevelAmt == user.RTLAuthority ? ApproverLevel : ++ApproverLevel;
                        LevelAmt = LevelAmt == user.RTLAuthority ? LevelAmt : user.RTLAuthority;
                    }
                    workflowTransactionApprovers = new WorkflowTransactionApprovers()
                    {
                        UpdateDate = DateTime.Now,
                        UserId = user.Id,
                        UserName= user.UserName,
                        WorkflowTransactionId= workflowTransaction.Id,
                        ApproverLevel= ApproverLevel

                    };
                     setupClaimsRequestcs = new SetupClaimsRequestcs()
                    {
                        TransactionType = CORE.Extensions.ClaimTransactionType.InsertWorkflowTransactionApprovers,
                        Request = workflowTransactionApprovers
                     };
                    workflowTransactionApprovers = Helpers.ExcutePostAPI<WorkflowTransactionApprovers>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

                }
            }
        }
        public static void UpdateClaimantStatus(Claimants claimant, AppSettings _appSettings)
        {


            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                ClaimantId = claimant.Id
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadAttachment,
                Request = mainSearchMC
            };
            var attachments = Helpers.ExcutePostAPI<List<Attachments>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");
            bool result = true;
            List<DocumentInfo> documentInfos1 = new List<DocumentInfo>();
            documentInfos1 = GetClaimantDocuments(claimant, _appSettings);
            foreach (var item in documentInfos1)
            {
                if (attachments.Where(p => p.DocumentSetupId == item.Id).ToList().Count == 0)
                {
                    result = false;
                    break;
                }
            }

            if (!result)
            {
                claimant.ClaimantStatus = (int)Enums.ClaimantStatus.MissingDocuments;
                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimants,
                    Request = claimant
                };
                claimant = Helpers.ExcutePostAPI<Claimants>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            }
            else if (claimant.ClaimantStatus == (int)Enums.ClaimantStatus.MissingDocuments)
            {
                claimant.ClaimantStatus = (int)Enums.ClaimantStatus.Operation;
                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimants,
                    Request = claimant
                };
                claimant = Helpers.ExcutePostAPI<Claimants>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
            }
        }

        public static List<DocumentInfo> GetClaimantDocuments(Claimants claimant, AppSettings _appSettings)
        {
            List<DocumentInfo> documentInfos1 = new List<DocumentInfo>();
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                ModuleId = 2
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadDocuments,
                Request = mainSearchMC
            };
            var documentInfos2 = Helpers.ExcutePostAPI<List<DocumentInfo>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");
            string[] ids = new string[1];
            List<docs> keyValuePairs = new List<docs>();
            foreach (var documentInfo in documentInfos2)
            {
                ids = new string[1];
                if (!string.IsNullOrEmpty(documentInfo.ClaimResult))
                {
                    ids = documentInfo.ClaimResult.Split(',');
                }
                foreach (var item in ids)
                {
                    if (!string.IsNullOrEmpty(item))
                        keyValuePairs.Add(new docs()
                        {
                            DocId = documentInfo.Id,
                            TransId = Convert.ToInt32(item)
                        });
                }
            }
            //Material
            if (claimant != null && claimant.DamageType.HasValue && (claimant.DamageType.Value == 1 || claimant.DamageType.Value == 4 || claimant.DamageType.Value == 5))
            {
                //Vehicle
                if (claimant.NatureofLoss.HasValue && claimant.NatureofLoss.Value == 1)
                {
                    foreach (var item in keyValuePairs.Where(p => p.TransId == 3))
                    {
                        documentInfos1.Add(documentInfos2.Where(p => p.Id == item.DocId).FirstOrDefault());
                    }
                }//Private
                else if (claimant.NatureofLoss.HasValue && claimant.NatureofLoss.Value == 2)
                {
                    foreach (var item in keyValuePairs.Where(p => p.TransId == 4))
                    {
                        documentInfos1.Add(documentInfos2.Where(p => p.Id == item.DocId).FirstOrDefault());
                    }
                }//Public
                else if (claimant.NatureofLoss.HasValue && claimant.NatureofLoss.Value == 3)
                {
                    foreach (var item in keyValuePairs.Where(p => p.TransId == 5))
                    {
                        documentInfos1.Add(documentInfos2.Where(p => p.Id == item.DocId).FirstOrDefault());
                    }
                }
            }
            //Death
            if (claimant != null && claimant.DamageType.HasValue && (claimant.DamageType.Value == 3 || claimant.DamageType.Value == 5 || claimant.DamageType.Value == 6 || claimant.DamageType.Value == 7))
            {
                foreach (var item in keyValuePairs.Where(p => p.TransId == 2))
                {
                    documentInfos1.Add(documentInfos2.Where(p => p.Id == item.DocId).FirstOrDefault());
                }
            }
            // Bodily
            if (claimant != null && claimant.DamageType.HasValue && (claimant.DamageType.Value == 2 || claimant.DamageType.Value == 6 || claimant.DamageType.Value == 7 || claimant.DamageType.Value == 10 || claimant.DamageType.Value == 4))
            {
                foreach (var item in keyValuePairs.Where(p => p.TransId == 1))
                {
                    documentInfos1.Add(documentInfos2.Where(p => p.Id == item.DocId).FirstOrDefault());
                }
            }
            return documentInfos1;
        }
        public static List<DocumentInfo> GetMissingDocuments(Claimants claimant, AppSettings _appSettings)
        {
            List<DocumentInfo> documentInfos = new List<DocumentInfo>();
            List<DocumentInfo> documentInfos1 = new List<DocumentInfo>();
            documentInfos = GetClaimantDocuments(claimant, _appSettings);
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                ClaimantId = claimant.Id
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadAttachment,
                Request = mainSearchMC
            };
            var attachments = Helpers.ExcutePostAPI<List<Attachments>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            foreach (var item in documentInfos)
            {
                if (attachments.Where(p => p.DocumentSetupId == item.Id).ToList().Count == 0)
                {
                    documentInfos1.Add(item);
                }
            }

            return documentInfos1;
        }

        public static void HandleSurveyorPhoto(Dictionary<IFormFile, string> photos, Survoyer survoyer,string UserName,AppSettings _appSettings) 
        {
            SetupClaimsRequestcs mainSearch = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
                Request = new MainSearchMC()
                {
                    Id = (int)survoyer.ClaimId
                }
            };
           var claims = Helpers.ExcutePostAPI<List<ClaimMaster>>(mainSearch, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            foreach (var item in photos)
            {
                Attachments attachment = new Attachments()
                {
                    ClaimantId = survoyer.ClaimantId,
                    ClaimId = survoyer.ClaimId,
                    CreationDate = DateTime.Now,
                    DocumentSetupId = 1017,
                    ModuleId = (int)Enums.DocumnetType.Surveyor,
                    FileName = Path.GetFileName(item.Key.FileName),
                    ContentType = item.Key.ContentType,
                    CreatedBy = UserName,
                    IsDeleted = false
                };
                SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateAttachment,
                    Request = attachment
                };
                attachment = Helpers.ExcutePostAPI<Attachments>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");
                string pathMDF = _appSettings.ClaimSubmissionPath;
                string fieNameWithExt = item.Value + "_" + Path.GetFileName(item.Key.FileName);
                string directory = Path.Combine(pathMDF, claims.FirstOrDefault().claims.ClaimNo,claims.Where(p=>p.claimants.Id==survoyer.ClaimantId).FirstOrDefault().claimants.Serial.ToString());
                bool folderExists = Directory.Exists(directory);
                if (!folderExists)
                    Directory.CreateDirectory(directory);

                string filePath = Path.Combine(directory, fieNameWithExt);
                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                     item.Key.CopyTo(fileStream);
                }
            }


        }
        public static List<Attachments> GeteClaimAttachment(ClaimSubmissionDocuments obj)
        {
            List<Attachments> attachments = new List<Attachments>();
            if (!string.IsNullOrEmpty(obj.AcciedentReport))
            {
                attachments.Add( new Attachments()
                {
                    ClaimantId = 0,
                    CreatedBy = "Online",
                    CreationDate = DateTime.Now,
                    ModuleId = 2,
                    ClaimId = 0,
                    FileName = obj.AcciedentReport,
                    ContentType = "",
                    DocumentSetupId = 1,
                    IsDeleted = false,
                });

            }
            if (!string.IsNullOrEmpty(obj.DA))
            {
                attachments.Add(new Attachments()
                {
                    ClaimantId = 0,
                    CreatedBy = "Online",
                    CreationDate = DateTime.Now,
                    ModuleId = 2,
                    ClaimId = 0,
                    FileName = obj.DA,
                    ContentType = "",
                    DocumentSetupId = 5,
                    IsDeleted = false,
                });
            }
            if (!string.IsNullOrEmpty(obj.IstimaraCopy))
            {
                attachments.Add(new Attachments()
                {
                    ClaimantId = 0,
                    CreatedBy = "Online",
                    CreationDate = DateTime.Now,
                    ModuleId = 2,
                    ClaimId = 0,
                    FileName = obj.IstimaraCopy,
                    ContentType = "",
                    DocumentSetupId = 2,
                    IsDeleted = false,
                });
            }
            if (!string.IsNullOrEmpty(obj.LicenseCopy))
            {
                attachments.Add(new Attachments()
                {
                    ClaimantId = 0,
                    CreatedBy = "Online",
                    CreationDate = DateTime.Now,
                    ModuleId = 2,
                    ClaimId = 0,
                    FileName = obj.LicenseCopy,
                    ContentType = "",
                    DocumentSetupId = 4,
                    IsDeleted = false,
                });
            }
            if (!string.IsNullOrEmpty(obj.Others))
            {
                attachments.Add(new Attachments()
                {
                    ClaimantId = 0,
                    CreatedBy = "Online",
                    CreationDate = DateTime.Now,
                    ModuleId = 2,
                    ClaimId = 0,
                    FileName = obj.Others,
                    ContentType = "",
                    DocumentSetupId = 1012,
                    IsDeleted = false,
                });
            }
            if (!string.IsNullOrEmpty(obj.IBAN))
            {
                attachments.Add(new Attachments()
                {
                    ClaimantId = 0,
                    CreatedBy = "Online",
                    CreationDate = DateTime.Now,
                    ModuleId = 2,
                    ClaimId =0,
                    FileName = obj.IBAN,
                    ContentType = "",
                    DocumentSetupId = 1011,
                    IsDeleted = false,
                });
            }

            return attachments;
        }

        public static void SaveFile(string obj,string Level)
        {
            try
            {
                //string path = @"E:\MC Logs\" + Level + ".txt";
                string path = @"D:\MotorClaims\Logs\" + Level + ".txt";
                System.IO.File.WriteAllText(path, obj);
            }
            catch (Exception)
            {
            }
        }

        public static string BankCode(string? IBAN)
        {
            string Digit = !string.IsNullOrEmpty(IBAN) ? IBAN.Substring(4, 2) : "20";
            switch (Digit)
            {
                case "10":return "B001";
                case "45":return "B002";
                case "65":return "B003";
                case "05":return "B004";
                case "55":return "B005";
                case "20":return "B006";
                case "40":return "B007";
                case "50":return "B008";
                case "80":return "B009";
                case "30":return "B010";
                case "15":return "B011";
                case "60":return "B012";
                case "83":return "B013";
                case "90":return "B014";
                case "95":return "B015";
                case "71":return "B016";
                case "75":return "B016";
                case "76":return "B018";
                case "81":return "B019";
                case "82":return "B020";
                case "87":return "B021";
                case "98":return "B022";
                case "84":return "B024";
                case "01":return "B025";
                case "86":return "B026";
                case "03":return "B027";
                 
            }
             return "B006";
        }

        public static string PlateMapping(string Code)
        {
            switch (Code)
            {
                case "1": return "A";
                case "2": return "B";
                case "3": return "D";
                case "4": return "J";
                case "5": return "H";
                case "6": return "E";
                case "7": return "G";
                case "8": return "X";
                case "9": return "T";
                case "10": return "K";
                case "11": return "Z";
                case "12": return "N";
                case "13": return "L";
                case "14": return "V";
                case "15": return "S";
                case "16": return "U";
                case "17": return "R";
            }
            return Code;
        }
        public static string TranslateText(string input, string languagePair)
        {
            string url = String.Format("http://www.google.com/translate_t?hl=en&ie=UTF8&text={0}&langpair={1}", input, languagePair);

            WebClient webClient = new WebClient();


            try
            {
                webClient.Encoding = System.Text.Encoding.UTF8;
                string result = webClient.DownloadString(url);
                int len = result.Length;
                result = result.Remove(0, result.IndexOf("id=result_box"));
                int len2 = result.Length;
                result = result.Remove(result.IndexOf("</span>"));
            }
            catch (Exception)
            {

                throw;
            }

            return "<span</span>";


        }

 
        public static void PublishWorkFlow(decimal TransactionAmount, Enums.WorkflowType workflowType,int claimTransactionsId,long ClaimId,int ClaimantID,string CreatedBy, List<Users> users,AppSettings _appSettings)
        {
            PublishWorkflow(TransactionAmount, workflowType, claimTransactionsId, ClaimId, ClaimantID, CreatedBy, users, _appSettings);
            RegisterHistory(_appSettings, ClaimId, "Update Reserve to " + TransactionAmount + " SAR by " + CreatedBy + " Pending with Approval", CreatedBy, ClaimantID);

        }
    }
}
