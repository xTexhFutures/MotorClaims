using CORE.DTOs.APIs.MotorClaim;
using CORE.DTOs.MotorClaim.Claims;
using CORE.Services;
using Newtonsoft.Json;
using PremiaProduction;
using PremiaEstimationProd;
using TPPremiaProd;
using PremiaSettlmentProd;
using CORE.DTOs.MotorClaim.Integrations.APIs;
using System.Globalization;

namespace MotorClaims.Models
{
    public static class PremiaIntegration
    {
        public static void CreateClaim(ClaimMaster claim, AppSettings _appSettings, out bool Result, out string error)
        {
            Result = true; error = string.Empty;
            //MainSearchMC mainSearchMC = new MainSearchMC()
            //{
            //    Id = (int)ClaimId
            //};
            //SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            //{
            //    TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaimsMaster,
            //    Request = mainSearchMC
            //};
            //var Lookups = Helpers.ExcutePostAPI<List<ClaimMaster>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            ClaimCreationRequestServiceClient serviceClient = new ClaimCreationRequestServiceClient();
            claimCreationResponseInfo claimCreationServiceResponse = new claimCreationResponseInfo();
            //ClaimCreationService claimCreationService = new ClaimCreationService();
            claimCreationRequestInfo claimCreationRequestInfo = new claimCreationRequestInfo();

            claimCreationRequestInfo.p_c_tran_sys_id = claim.claims.ClaimNo + "/" + claim.claimants.Serial;
            claimCreationRequestInfo.p_c_insert_update_flag = claim.claims.Id > 0 && claim.claims.PremiaClaimId.HasValue && claim.claims.PremiaClaimId.Value > 0 ? "U" : "I";
            claimCreationRequestInfo.p_c_claim_sys_id = claim.claims.Id > 0 ? (claim.claims.PremiaClaimId.HasValue ? claim.claims.PremiaClaimId.Value.ToString() : "") : "";
            claimCreationRequestInfo.p_c_pol_no = claim.claims.PolicyNo;
            claimCreationRequestInfo.p_c_clm_type = "NORMAL";
            claimCreationRequestInfo.p_c_loss_date = claim.claims.DateOfLoss.ToString("dd-MMM-yyyy", new CultureInfo("en"));
            claimCreationRequestInfo.p_c_intmt_date = claim.claims.RegistrationDate.ToString("dd-MMM-yyyy", new CultureInfo("en"));
            claimCreationRequestInfo.p_c_nat_of_loss = "TPPD";//Nature of loss
            claimCreationRequestInfo.p_c_loss_desc = !string.IsNullOrEmpty(claim.claims.Notes) ? claim.claims.Notes : "Loss Desc";
            claimCreationRequestInfo.p_c_acdnt_location = "RY";//City
            claimCreationRequestInfo.p_c_cause_of_loss = "506";//Cause of Loss
            claimCreationRequestInfo.p_c_user_division = claim.claims.BranchId.ToString();//City Division
            claimCreationRequestInfo.p_c_curr_code = "SAR";

            claimCreationRequestInfo.p_c_clm_flexi_01 = claim.claims.AccidentNo;
            claimCreationRequestInfo.p_c_clm_flexi_02 = claim.claimants.DriverName;
            claimCreationRequestInfo.p_c_clm_flexi_07 = claim.claimants.MobileNo;
            claimCreationRequestInfo.p_c_clm_flexi_06 = claim.claimants.DriverNationalId;

            SearchingObj searchingObj = new SearchingObj()
            {
                ClaimId = claim.claims.Id,
                SequenceNo = claim.claims.SequanceNo,
                PolicyNo = claim.claims.PolicyNo,
            };
            searchingObj = Helpers.ExcutePostAPI<SearchingObj>(searchingObj, _appSettings.APIHubPrefix + "api/MotorClaim/GetCoverDetails");
           
            claimRequestRiskDtlsIn claimRequestRiskDtlsIn = new claimRequestRiskDtlsIn()
            {
                c_r_section_code = searchingObj.ProductCode + "01",
                c_r_claim_risk_id = searchingObj.RiskId,
                c_r_clamp_pol_no = claim.claims.PolicyNo,
                c_r_cover_code = searchingObj.CoverCode,//Policy Covers List
                c_r_fault_perc = claim.claimants.OurPercent.HasValue ? claim.claimants.OurPercent.Value.ToString() : "0",
                c_r_flexi_01 = claim.claims.Id > 0 && claim.claims.PremiaClaimId.HasValue && claim.claims.PremiaClaimId.Value > 0 ? "U" : "I"

            };
            claimCreationRequestInfo.p_c_risk_dtls_in = new claimRequestRiskDtlsIn[1];
            claimCreationRequestInfo.p_c_risk_dtls_in[0] = claimRequestRiskDtlsIn;


            claimCreationRequestInfo.p_c_clm_tp_in = new claimRequestTpInIn[1];




            claimRequestTpInIn claimRequestTpInIn = new claimRequestTpInIn()
            {
                c_tp_benf_name_arab = claim.claimants.BenefecieryName,
                c_tp_benf_name_eng = !string.IsNullOrEmpty(claim.claimants.BenefecieryNameEN) ? claim.claimants.BenefecieryNameEN : "AAAAABBBBB",
                c_tp_driver_name = claim.claimants.DriverName,
                c_tp_digit = !string.IsNullOrEmpty(claim.claimants.PlateNo) ? "" : "",
                c_tp_arab = !string.IsNullOrEmpty(claim.claimants.PlateNo) ? "" : "",
                c_tp_make = claim.claimants.MakeId.HasValue ? claim.claimants.MakeId.Value.ToString() : "",
                c_tp_model = claim.claimants.ModelId.HasValue ? claim.claimants.ModelId.Value.ToString() : "",
                c_tp_sequence_no = claim.claimants.SequenceNo,
                c_tp_mobile_no = claim.claimants.MobileNo,
                c_tp_insurer_name = claim.claimants.OwnerName,
                c_tp_benificiary_id = claim.claimants.OwnerNationalId,
                c_tp_more_details = claim.claimants.NationalId,
                c_tp_mode_of_payment = "BT",//Mode of Payment
                c_tp_bank_name = !string.IsNullOrEmpty(claim.claimants.BankName) ? claim.claimants.BankName : "B018",// claim.claimants.BankName,
                c_tp_iban_no = claim.claimants.Iban,
                c_DOB = claim.claimants.DriverBirthDate.HasValue ? claim.claimants.DriverBirthDate.Value.ToString("dd/MM/yyyy") : "01/01/1900",
                c_tp_party_ref_no = claim.claimants.OwnerName,
                c_tp_flex_02 = claim.claimants.DriverNationalId,
                c_tp_flex_03 = "A@A.com",//Owner Email
                c_tp_flex_04 = "0",
                c_tp_flex_01 = claim.claimants.PremiaClaimId.HasValue ? "U" : "I",
                c_tp_cfd_sys_id = claim.claimants.PremiaClaimId.HasValue ? claim.claimants.PremiaClaimId.Value.ToString() : "",
                c_tp_clm = claim.claimants.PremiaClaimId.HasValue ? (claim.claims.PremiaClaimId.HasValue ? claim.claims.PremiaClaimId.Value.ToString() : "") : "",

            };


            claimCreationRequestInfo.p_c_clm_tp_in[0] = claimRequestTpInIn;
            //claimCreationService.ClaimCreationRequestInfo = claimCreationRequestInfo;
            var yy = JsonConvert.SerializeObject(claimCreationRequestInfo);
            Helpers.SaveFile(yy, claim.claims.ClaimNo + "_A");
            claimCreationServiceResponse = serviceClient.ClaimCreationService(claimCreationRequestInfo);

            var y1y = JsonConvert.SerializeObject(claimCreationServiceResponse);
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs();
            Claims clm = new Claims();
            if (claimCreationServiceResponse != null && !string.IsNullOrEmpty(claimCreationServiceResponse.p_c_clm_no))
            {
                Result = true; error = claimCreationServiceResponse.p_c_clm_no;
                clm = claim.claims;
                clm.PremiaClaimId = !string.IsNullOrEmpty(claimCreationServiceResponse.p_c_clm_sys_id) ? Convert.ToInt32(claimCreationServiceResponse.p_c_clm_sys_id) : null;
                clm.PremiaClaimSegmentCode = claimCreationServiceResponse.p_c_clm_no;
                clm.PremiaRiskLamp = claimCreationServiceResponse.p_C_RISK_DTLS_OUT.Count() > 0 ? Convert.ToInt32(claimCreationServiceResponse.p_C_RISK_DTLS_OUT.FirstOrDefault().c_lmap_sys_id) : null;
            }
            else
            {
                Result = false; error = claimCreationServiceResponse.p_C_ERROR_MSG[0].err_desc;
                clm = claim.claims;
                clm.PremiaClaimSegmentCode = error;


            }

            setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaim,
                Request = clm
            };
            clm = Helpers.ExcutePostAPI<Claims>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

        }

        public static void CreateTPClaim(ClaimMaster claim, AppSettings _appSettings, out bool Result, out string error)
        {
            Result = true; error = string.Empty;

            ThirdPartyCreationRequestServiceClient serviceClient = new ThirdPartyCreationRequestServiceClient();
            ThirdPartyCreationServiceResponse claimCreationServiceResponse = new ThirdPartyCreationServiceResponse();
            ThirdPartyCreationService claimCreationRequestInfo = new ThirdPartyCreationService();
            thirdPartyCreationRequestInfo request = new thirdPartyCreationRequestInfo();

            request.p_c_tp_tran_sys_id = claim.claims.ClaimNo + "/" + claim.claimants.Serial;
            request.p_c_tp_insert_update_flag = claim.claimants.Id > 0 && claim.claimants.PremiaClaimId.HasValue && claim.claimants.PremiaClaimId.Value > 0 ? "U" : "I";
            request.p_c_claim_sys_id = claim.claims.PremiaClaimId.Value.ToString();

            thirdPartyRequestTpInIn[] thirdPartyRequestTpInIns = new thirdPartyRequestTpInIn[1];


            List<Attachments> attachments = new List<Attachments>();
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                ClaimantId = Convert.ToInt32(claim.claimants.Id)
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadAttachment,
                Request = mainSearchMC
            };
            attachments = Helpers.ExcutePostAPI<List<Attachments>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/SetupMotorClaim");

            SearchLookUp searchLookUp = new SearchLookUp()
            {
                MajorCode = SystemEnums.NajmMapping,
                Id = claim.claimants.ModelId
            };
            var lookupTables = Helpers.ExcutePostAPI<List<LookupTable>>(searchLookUp, _appSettings.APIHubPrefix + "api/MotorClaim/Loadlookups");


            thirdPartyRequestTpInIn TP = new thirdPartyRequestTpInIn();


            TP.c_tp_sys_id = claim.claimants.PremiaClaimId.HasValue && claim.claimants.PremiaClaimId.Value > 0 ? claim.claimants.PremiaClaimId.ToString() : null;
            TP.c_tp_clm_sys_id = claim.claims.PremiaClaimId.ToString();
            TP.c_tp_cfd_sys_id = claim.claims.PremiaClaimId.ToString();
            TP.c_TP_ARAB = string.IsNullOrEmpty(claim.claimants.PlateChar1) && string.IsNullOrEmpty(claim.claimants.PlateChar2) && string.IsNullOrEmpty(claim.claimants.PlateChar3) ? null : Helpers.PlateMapping(claim.claimants.PlateChar1) + " " + Helpers.PlateMapping(claim.claimants.PlateChar2) + " " + Helpers.PlateMapping(claim.claimants.PlateChar3);
            TP.c_tp_driver_name = claim.claimants.DriverName;
            TP.c_tp_benf_name_eng = !string.IsNullOrEmpty(claim.claimants.BenefecieryNameEN) ? claim.claimants.BenefecieryNameEN : "AAAABBBBBBBBB";// Helpers.TranslateText( claim.claimants.BenefecieryName, "ar|en"),
            TP.c_tp_benf_name_arab = claim.claimants.BenefecieryName;
            TP.c_tp_digit = !string.IsNullOrEmpty(claim.claimants.PlateNo) ? claim.claimants.PlateNo : null;
            TP.c_tp_make = lookupTables != null && lookupTables.Count > 0 && claim.claimants.ModelId.HasValue && claim.claimants.ModelId.Value > 0 ? lookupTables.FirstOrDefault().NameEnglish : "BMW0002";//MakeCode Premia,
            TP.c_tp_model = claim.claimants.Manifacturing.HasValue ? claim.claimants.Manifacturing.ToString() : "";
            TP.c_tp_sequence_no = !string.IsNullOrEmpty(claim.claimants.SequenceNo) ? claim.claimants.SequenceNo : "111111000";
            TP.c_tp_mobile_no = !string.IsNullOrEmpty(claim.claimants.MobileNo) ? (claim.claimants.MobileNo.Length < 10 ? "0" + claim.claimants.MobileNo : (claim.claimants.MobileNo.Length > 10 ? "0" + claim.claimants.MobileNo.Substring(3, 9) : claim.claimants.MobileNo)) : null;
            TP.c_tp_insurer_name = !string.IsNullOrEmpty(claim.claimants.InsuranceCompanyName) ? claim.claimants.InsuranceCompanyName : "null";
            TP.c_tp_more_details = claim.claimants.OwnerName;
            TP.c_tp_benificiary_id = claim.claimants.OwnerNationalId;
            TP.c_tp_discharge_remarks = "02";
            TP.c_tp_mode_of_pay = "BT";
            TP.c_tp_bank_name = Helpers.BankCode(claim.claimants.Iban);//Bank Name Mapping
            TP.c_tp_iban_no = !string.IsNullOrEmpty(claim.claimants.Iban) ? claim.claimants.Iban.ToUpper() : "SA1111111111111111111111";
            TP.c_TP_PARTY_REF_NO = claim.claimants.OwnerName;
            TP.c_DOB = claim.claimants.DriverBirthDate.HasValue ? claim.claimants.DriverBirthDate.Value.ToString("dd/MM/yyyy", new CultureInfo("en")) : DateTime.Now.AddYears(-18).ToString("dd/MM/yyyy", new CultureInfo("en"));
            TP.c_tp_flex_02 = claim.claimants.DriverNationalId;
            TP.c_tp_flex_03 = "A@A.com";
            TP.c_tp_flex_04 = "0";
            //c_tp_flex_05 = DateTime.Now.ToString("dd-MMM-yyyy"),
            //c_tp_flex_06 = DateTime.Now.ToString("dd-MMM-yyyy"),

            TP.c_tp_flex_07 = "02~1~0~" + (attachments != null && attachments.Count > 0 ? attachments.FirstOrDefault().CreationDate.ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy")) + "~NA~NA~1~" + (attachments != null && attachments.Count > 0 ? attachments.FirstOrDefault().CreationDate.ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy")) + "~NA~Verified";
            TP.c_tp_flex_08 = "03~1~0~" + (attachments != null && attachments.Count > 0 ? attachments.FirstOrDefault().CreationDate.ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy")) + "~NA~NA~1~" + (attachments != null && attachments.Count > 0 ? attachments.FirstOrDefault().CreationDate.ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy")) + "~NA~Verified";
            TP.c_tp_flex_09 = "01~1~0~" + (attachments != null && attachments.Count > 0 ? attachments.FirstOrDefault().CreationDate.ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy")) + "~NA~NA~1~" + (attachments != null && attachments.Count > 0 ? attachments.FirstOrDefault().CreationDate.ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy")) + "~NA~Verified";

            //foreach (var item in attachments)
            //{
            //    if (item.DocumentSetupId == 1)
            //    {
            //        TP.c_tp_flex_07 = "02~1~0~" + item.CreationDate.ToString("dd/MM/yyyy") + "~NA~NA~1~" + item.CreationDate.ToString("dd/MM/yyyy") + "~NA";
            //    //}
            //    //else if (item.DocumentSetupId == 2)
            //    //{
            //        //05
            //        TP.c_tp_flex_08 = "03~1~0~" + item.CreationDate.ToString("dd/MM/yyyy") + "~NA~NA~1~" + item.CreationDate.ToString("dd/MM/yyyy") + "~NA";
            //    //}
            //    //else if (item.DocumentSetupId == 4)
            //    //{
            //        //06
            //        TP.c_tp_flex_09 = "01~1~0~" + item.CreationDate.ToString("dd/MM/yyyy") + "~NA~NA~1~" + item.CreationDate.ToString("dd/MM/yyyy") + "~NA";
            //    }
            //    //if (item.DocumentSetupId == 5)
            //    //{
            //    //    TP.c_tp_flex_10 = "04~1~0~" + item.CreationDate.ToString("dd/MM/yyyy") + "~NA~NA~1~" + item.CreationDate.ToString("dd/MM/yyyy") + "~NA";
            //    //}
            //    //if (item.DocumentSetupId == 1011)
            //    //{
            //    //    TP.c_tp_flex_11 = "16~1~0~" + item.CreationDate.ToString("dd/MM/yyyy") + "~NA~NA~1~" + item.CreationDate.ToString("dd/MM/yyyy") + "~NA";
            //    //}
            //    //if (item.DocumentSetupId == 11)
            //    //{
            //    //    TP.c_tp_flex_12 = "10~1~0~" + item.CreationDate.ToString("dd/MM/yyyy") + "~NA~NA~1~" + item.CreationDate.ToString("dd/MM/yyyy") + "~NA";
            //    //}
            //    //if (item.DocumentSetupId == 8)
            //    //{
            //    //    TP.c_tp_flex_13 = "19~1~0~" + item.CreationDate.ToString("dd/MM/yyyy") + "~NA~NA~1~" + item.CreationDate.ToString("dd/MM/yyyy") + "~NA";
            //    //}
            //    //if (item.DocumentSetupId == 12)
            //    //{
            //    //    TP.c_tp_flex_14 = "15~1~0~" + item.CreationDate.ToString("dd/MM/yyyy") + "~NA~NA~1~" + item.CreationDate.ToString("dd/MM/yyyy") + "~NA";
            //    //}
            //}
            thirdPartyRequestTpInIns[0] = TP;
           
            
            request.p_c_clm_tp_in = thirdPartyRequestTpInIns;

            claimCreationRequestInfo.ThirdPartyCreationRequestInfo = request;
            var yy = JsonConvert.SerializeObject(claimCreationRequestInfo);
            Helpers.SaveFile(yy, claim.claims.ClaimNo + "_B");
            claimCreationServiceResponse = serviceClient.ThirdPartyCreationService(claimCreationRequestInfo);

            var y1y = JsonConvert.SerializeObject(claimCreationServiceResponse);
            setupClaimsRequestcs = new SetupClaimsRequestcs();
            Claimants clm = new Claimants();
            if (claimCreationServiceResponse != null && claimCreationServiceResponse.ThirdPartyCreationResponse.p_C_CLM_TP_OUT.Count() > 0 && !string.IsNullOrEmpty(claimCreationServiceResponse.ThirdPartyCreationResponse.p_C_CLM_TP_OUT.FirstOrDefault().tppd_sys_id))
            {
                Result = true; error = claimCreationServiceResponse.ThirdPartyCreationResponse.p_c_tp_sys_id;
                clm = claim.claimants;
                clm.PremiaClaimId = !string.IsNullOrEmpty(claimCreationServiceResponse.ThirdPartyCreationResponse.p_C_CLM_TP_OUT.FirstOrDefault().tppd_sys_id) ? Convert.ToInt32(claimCreationServiceResponse.ThirdPartyCreationResponse.p_C_CLM_TP_OUT.FirstOrDefault().tppd_sys_id) : null;
                clm.PremiaStatus = claimCreationServiceResponse.ThirdPartyCreationResponse.p_C_ERROR_MSG.Count() > 0 ? claimCreationServiceResponse.ThirdPartyCreationResponse.p_C_ERROR_MSG.FirstOrDefault().err_desc : null;
                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimants,
                    Request = clm
                };
                clm = Helpers.ExcutePostAPI<Claimants>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            }
            else
            {
                Result = false;
                clm = claim.claimants;
                clm.PremiaStatus = claimCreationServiceResponse.ThirdPartyCreationResponse.p_C_ERROR_MSG.Count() > 0 ? claimCreationServiceResponse.ThirdPartyCreationResponse.p_C_ERROR_MSG.FirstOrDefault().err_desc : null;
                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertUpdateClaimants,
                    Request = clm
                };
                clm = Helpers.ExcutePostAPI<Claimants>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");
                error = claimCreationServiceResponse.ThirdPartyCreationResponse.p_C_ERROR_MSG[0].err_desc;
            }
        }

        public static void CreateClaimEstimation(ClaimMaster claim, ReserveObj reserve, decimal Amount, int TransType, bool IsClose, AppSettings _appSettings, out bool Result, out string? error)
        {
            Result = true; error = string.Empty;

            EstimateCreationRequestServiceClient serviceClient = new EstimateCreationRequestServiceClient();
            EstimateCreationServiceResponse claimCreationServiceResponse = new EstimateCreationServiceResponse();
            EstimateCreationService claimCreationRequestInfo = new EstimateCreationService();
            estimateCreationRequestInfo request = new estimateCreationRequestInfo();

            request.p_C_EST_TRAN_SYS_ID = claim.claims.ClaimNo + "/" + claim.claimants.Serial;
            request.p_C_EST_INSERT_UPDATE_FLAG = IsClose ? "U" : "I";
            request.p_C_EST_CLM_SYS_ID = claim.claims.PremiaClaimId.Value.ToString();

            claimEstimateDtls[] claimEstimateDtls = new claimEstimateDtls[1];
            claimEstimateDtls estimate = new claimEstimateDtls()
            {
                c_EST_CLMAP_SYS_ID = claim.claims.PremiaRiskLamp.HasValue && claim.claims.PremiaRiskLamp.Value > 0 ? claim.claims.PremiaRiskLamp.ToString() : null,
                c_EST_PROVISION_DT = reserve.reserve.Creationdate < DateTime.Now ? DateTime.Now.ToString("dd-MMM-yyyy", new CultureInfo("en")) : reserve.reserve.Creationdate.ToString("dd-MMM-yyyy", new CultureInfo("en")),
                c_EST_CODE = reserve.reserveDetails.Where(p => p.ReserveType == TransType).FirstOrDefault()?.PremiaCode,//mapping with reserve codes
                c_EST_CUSTOMER_CODE = "027000001",//lookup customer master  
                c_EST_CURR_CODE = "SAR",
                c_EST_PROVISION_AMT = Convert.ToDecimal(Amount).ToString(),
                c_EST_PARTY_REF_NO = claim.claimants.PremiaClaimId.HasValue && claim.claimants.PremiaClaimId.Value > 0 ? claim.claimants.PremiaClaimId.ToString() : null,
                c_EST_APPR_YN = "1",
                c_EST_CLOSE_YN = IsClose ? "1" : "0",
                c_EST_SYS_ID = IsClose ? reserve.reserveDetails.Where(p => p.ReserveType == TransType).FirstOrDefault()?.PremiaClaimId.ToString() : null
            };
            claimEstimateDtls[0] = estimate;

            request.p_C_EST_DTLS_IN = claimEstimateDtls;

            claimCreationRequestInfo.EstimateCreationRequestInfo = request;
            try
            {
                claimCreationServiceResponse = serviceClient.EstimateCreationService(claimCreationRequestInfo);
                var yy = JsonConvert.SerializeObject(claimCreationRequestInfo);
                Helpers.SaveFile(yy, claim.claims.ClaimNo + "_EstimationA");
                SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs();

                if (claimCreationServiceResponse != null && claimCreationServiceResponse.EstimateCreationService.p_C_EST_DTLS_OUT.Count() > 0)
                {
                    estimateClampDetailsOut[] Ids = claimCreationServiceResponse.EstimateCreationService.p_C_EST_DTLS_OUT;

                    Ids = Ids.OrderByDescending(p => p.CE_SYS_ID).ToArray<estimateClampDetailsOut>();
                    Result = true; error = Ids[0].CE_SYS_ID;

                    int PremiaId = !string.IsNullOrEmpty(Ids[0].CE_SYS_ID) ? Convert.ToInt32(Ids[0].CE_SYS_ID) : 0;
                    ReserveDetails reserveDetails = reserve.reserveDetails.Where(p => p.ReserveType == TransType).FirstOrDefault();

                    reserveDetails.PremiaClaimId = PremiaId;


                    setupClaimsRequestcs = new SetupClaimsRequestcs()
                    {
                        TransactionType = CORE.Extensions.ClaimTransactionType.InsertReserveDetail,
                        Request = reserveDetails
                    };
                    reserveDetails = Helpers.ExcutePostAPI<ReserveDetails>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

                }
                else
                {
                    Result = false; error = claimCreationServiceResponse != null && claimCreationServiceResponse.EstimateCreationService != null && claimCreationServiceResponse.EstimateCreationService.p_C_ERROR_MSG.Count() > 0 ? claimCreationServiceResponse.EstimateCreationService.p_C_ERROR_MSG.FirstOrDefault().ERR_DESC : null;
                }
            }
            catch (Exception ex)
            {
                Result = false;
                error = ex.Message;
            }


        }

        public static void CreateClaimSettlement(ClaimMaster claim, Settlements settlements, ReserveDetails reserve, AppSettings _appSettings, out bool Result, out string error)
        {
            Result = true; error = string.Empty;

            SettlementCreationRequestServiceClient serviceClient = new SettlementCreationRequestServiceClient();
            SettlementCreationRequestServiceResponse claimCreationServiceResponse = new SettlementCreationRequestServiceResponse();
            SettlementCreationRequestService1 claimCreationRequestInfo = new SettlementCreationRequestService1();
            settlementCreationRequestInfo request = new settlementCreationRequestInfo();

            request.c_setl_tran_sys_id = claim.claims.ClaimNo + "/" + claim.claimants.Serial;
            request.c_setl_insert_update_flag = "I";
            request.c_setl_clm_sys_id = claim.claims.PremiaClaimId.Value.ToString();
            request.c_setl_claim_close_yn = "0";
            request.c_setl_close_reason_code = "CLM-CLO-003";//Close reason Mapping
            request.c_setl_close_remarks = "Fully Settled";
            settlementRequestIn[] settlementRequestIns = new settlementRequestIn[1];
            settlementRequestIn settlement = new settlementRequestIn()
            {
                c_setl_ce_sys_id = reserve.PremiaClaimId.ToString(),
                c_setl_iban_no = claim.claimants.Iban,
                c_setl_date = DateTime.Now.ToString("dd-MMM-yyyy", new CultureInfo("en")),
                c_setl_est_code = reserve.PremiaCode,//mapping with reserve codes
                c_setl_cust_code = "027000001",//lookup customer master
                c_setl_payee_code = "027000001",//lookup customer master
                c_setl_remarks = "Settlement from Webservice",
                c_setl_cust_name = claim.claimants.BenefecieryName,
                c_setl_mode_of_payment = "BT",
                c_setl_amt = reserve.Amount.ToString(),
                c_setl_benf_id = claim.claimants.OwnerNationalId,
                c_setl_yn = "1",
                c_setl_final_yn = "1",
                c_setl_bank_name = Helpers.BankCode(claim.claimants.Iban),//Bank name integration
                c_setl_benf_name_eng = claim.claimants.BenefecieryName,
                c_setl_benf_name_arab = claim.claimants.BenefecieryName,
                c_setl_clmap_sys_id_in = claim.claims.PremiaRiskLamp.ToString(),
                c_setl_flex_29 = "INV" + DateTime.Now.Minute + "-" + DateTime.Now.Second + "-" + reserve.Id,
                c_setl_flex_30 = DateTime.Now.ToString("dd-MMM-yyyy", new CultureInfo("en")),
            };
            settlementRequestIns[0] = settlement;

            request.c_setl_clm_setl_in = settlementRequestIns;

            claimCreationRequestInfo.SettlementCreationRequestInfo = request;
            claimCreationServiceResponse = serviceClient.SettlementCreationRequestService(claimCreationRequestInfo);
            var yy = JsonConvert.SerializeObject(claimCreationRequestInfo);
            Helpers.SaveFile(yy, claim.claims.ClaimNo + "_settlementsA");
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs();

            if (claimCreationServiceResponse != null && claimCreationServiceResponse.SettlementCreationResponse.c_clm_setl_out.Count() > 0 && !string.IsNullOrEmpty(claimCreationServiceResponse.SettlementCreationResponse.c_clm_setl_out[0].cs_sys_id))
            {
                Result = true; error = claimCreationServiceResponse.SettlementCreationResponse.c_clm_setl_out[0].cs_ce_sys_id;
                settlementResponseOut[] Ids = claimCreationServiceResponse.SettlementCreationResponse.c_clm_setl_out;
                Ids = Ids.OrderByDescending(p => p.cs_ce_sys_id).ToArray<settlementResponseOut>();

                int PremiaId = !string.IsNullOrEmpty(Ids[0].cs_sys_id) ? Convert.ToInt32(Ids[0].cs_sys_id) : 0;
                settlements.PremiaClaimId = PremiaId;

                setupClaimsRequestcs = new SetupClaimsRequestcs()
                {
                    TransactionType = CORE.Extensions.ClaimTransactionType.InsertSettlement,
                    Request = settlements
                };
                settlements = Helpers.ExcutePostAPI<Settlements>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            }
            else
            {
                Result = false; error = claimCreationServiceResponse.SettlementCreationResponse.p_c_error_msg[0].ERR_DESC;
            }
        }


    }
}
