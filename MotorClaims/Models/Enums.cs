using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Xml.Linq;

namespace MotorClaims.Models
{
    public static class EnumExtensions
    {
        public static string DisplayName(this Enum value)
        {
            Type enumType = value.GetType();
            var enumValue = Enum.GetName(enumType, value);
            MemberInfo member = enumType.GetMember(enumValue)[0];

            var attrs = member.GetCustomAttributes(typeof(DisplayAttribute), false);
            var outString = ((DisplayAttribute)attrs[0]).Name;

            if (((DisplayAttribute)attrs[0]).ResourceType != null)
            {
                outString = ((DisplayAttribute)attrs[0]).GetName();
            }

            return outString;
        }
    }
    public static class Enums
    {
        public enum TeamAssign
        {
            Operations = 2,
            Surveyors = 3,
            Fraud = 4,
            Salvage=11,
        }
        public enum DocumnetType
        {
            Claim = 1,
            Claimant = 2,
            Fraud = 3,
            Reserve = 4,
            Payment = 5,
            Surveyor = 6
        }

        public enum ClaimTransactionTypes
        {
            Reserve = 1,
            Payment = 2,
            Recovery = 3,
            Collection = 4,
        }
        public enum FraudLevel
        {
            Error = 4,
            Warning = 3,
            Low = 2,
            None = 1
        }
        public enum ClaimReportType
        {
            Najm = 1,
            Basher = 2,
            Manual = 3
        }
        public enum YakeenPlace
        {
            Policy = 1,
            Quotation = 0
        }
        public enum Gender
        {
            Male = 1,
            Female = 2
        }
        public enum MeritalStatus
        {
            Single = 1,
            Married = 2,
            Widow = 5,
            Divorced = 4
        }
        public enum Status
        {
            [Display(Name = "In Progress")]
            InProgress = 1,
            [Display(Name = "Waiting Approval")]
            WaitingApproval = 2,
            Paid = 3,
            Rejected = 4
        }
        public enum Roles
        {
            Admin = 1,
            UWApproval = 2,
            Sales = 3,
            Broker = 4,
            Agent = 5,
            Finance = 6,
            Approval = 7,
            IT = 8
        }

        public enum ClaimantStatus
        {
            NeedMoreInfo = 0,
            MissingDocuments = 1,
            Operation = 2,
            Surveyor = 3,
            Payment = 4,
            Workshop = 5,
            Closed = 6,
            InProgress = 7,
            Rejected = 8,
            Reception = 9,
            ReOpen = 10,
            Salvage=11
        }


        public enum Lookups
        {
            City = 80,
            CauseOfLoss = 1,
            ClaimantType = 81,
            DamageType = 3
        }
        public enum ClaimStatus
        {
            Pendding = 0,
            Closed=2,
            InProgress = 1,
            Rejected = 3
        }
        public enum TransactionCategory
        {
            InitialReserve = 1,
            TowingCost = 2
        }

        public enum SMSTemplates
        {
            Test = 1
        }

        public enum SurveyorActions
        {
            [Display(Name = "Pending for Surveyor")]
            PendingforSurveyor = 1,
            [Display(Name = "Waiting estimations")]
            Waitingestimations = 2,
            [Display(Name = "Sent to service unit")]
            Senttoserviceunit = 3,
            [Display(Name = "Waiting Customer to deliver his vehicle")]
            WaitingCustomertodeliverhisvehicle = 4

        }
    }
}
