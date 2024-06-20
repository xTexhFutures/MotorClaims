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
            Salvage = 11,
        }

        public enum SettlementResult
        {
            FullySettled = 1,
            Partially = 2
        }
        public enum DocumnetType
        {
            Claim = 1,
            Claimant = 2,
            Fraud = 3,
            Reserve = 4,
            Payment = 5,
            Surveyor = 6,
            Operations = 7
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
            Other = 3
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
            [Display(Name = "Need More Info")]
            NeedMoreInfo = 0,
            [Display(Name = "Missing Documents")]
            MissingDocuments = 1,
            [Display(Name = "Operation")]
            Operation = 2,
            [Display(Name = "Surveyor")]
            Surveyor = 3,
            [Display(Name = "Payment")]
            Payment = 4,
            [Display(Name = "Workshop / Agency")]
            Workshop = 5,
            [Display(Name = "Closed")]
            Closed = 6,
            [Display(Name = "In Progress")]
            InProgress = 7,
            [Display(Name = "Rejected")]
            Rejected = 8,
            [Display(Name = "Reception")]
            Reception = 9,
            [Display(Name = "Re-Open")]
            ReOpen = 10,
            [Display(Name = "Salvage")]
            Salvage = 11,
            [Display(Name = "Waiting Salvage Approvals")]
            WaitingSalvageApprovals = 12,
            [Display(Name = "Waiting Settelment Approvals")]
            WaitingSettelmentApprovals = 13
        }


        public enum Lookups
        {
            City = 80,
            CauseOfLoss = 1,
            ClaimantType = 81,
            DamageType = 3,
            ReserveCodes = 75,
            NajmMapping = 91
        }
        public enum ClaimStatus
        {
            Pendding = 0,
            Closed = 2,
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

        public enum VehicleLocation
        {
            AgencyOrWorkshop = 1,
            Client = 2,
            Branch = 3,
            Provider = 4,
            Workshop = 5,
            Other = 6
        }

        public enum TowingStatus
        {
            [Display(Name = "Client Refuse")]
            ClientRefuse = 1,
            [Display(Name = "Vehicle Not Found in the Location")]
            VehicleNotFound = 2,
            [Display(Name = "Arrested In Baladeyah")]
            ArrestedInBaladeyah = 3,
            [Display(Name = "Arrested In Morror")]
            ArrestedInMorror = 4,
            [Display(Name = "Wrong Phone Number")]
            WrongPhone = 5,
            [Display(Name = "Client Not Coordinating")]
            ClientNotCoordinating = 6,
            [Display(Name = "Others")]
            Others = 7,
        }

        public enum MissingPartsList
        {
            [Display(Name = "Front Plate")]
            FrontPlate = 1,
            [Display(Name = "Rear Plate")]
            RearPlate = 2,
            [Display(Name = "Spare Tire")]
            SpareTire = 3,
            [Display(Name = "Car Key")]
            CarKey = 4,
            [Display(Name = "Tires")]
            Tires = 5,
            [Display(Name = "Rims")]
            Rims = 6,
            [Display(Name = "Recorder")]
            Recorder = 7,
            [Display(Name = "Others")]
            Others = 8
        }


        public enum RecoveryStatus
        {
            [Display(Name = "Open")]
            Open = 1,
            [Display(Name = "Partially Collected")]
            PartiallyCollected = 2,
            [Display(Name = "Collected")]
            Collected = 3
        }
        public enum SurvorStatus
        {
            [Display(Name = "In Progress")]
            InProgress = 1,
            [Display(Name = "Completed")]
            Completed = 2,
            [Display(Name = "Fraud")]
            Fraud = 3,
            [Display(Name = "Total Loss")]
            TotalLoss = 4
        }
        public enum CollectionType
        {
            Fully = 1,
            Partial = 2
        }

        public enum WorkflowStatus
        {
            Pending = 0,
            Approved = 1,
            Rejected = 2
        }

        public enum WorkflowType
        {
            Reserve = 1,
            RepairOrder = 2,
            TotalLoss = 3,
            SettelmentAutherity = 4
        }
        public enum ReserveType
        {
            SpareParts = 1,
            LaborCost = 2, 
            OtherCost = 3,
            SheikhAlmaared = 4,
            TowingCost = 5
        }
        public enum RepairConditions
        {
            Agency = 1,
            Workshop = 2
        }

        public enum SettlementType
        {
            RepaireInvoice = 1,
            TP = 2,
            TowingCharge = 3,
            TotalLoss = 4
        }
    }
}
