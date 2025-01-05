using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;

namespace MotorClaims.Models
{
    public class Search
    {
        public string? plate { get; set; }
        public string? sequence { get; set; }
        public string? claimno { get; set; }
        public string? nationalid { get; set; }
        public string? mobile { get; set; }
        public string? complain { get; set; }
        public string? custom { get; set; }
        public string? chassis { get; set; }
        public string? policy { get; set; }
    }
    public class MissingDocuments
    {
        public int? Id { get; set; }
        public long? ClaimId { get; set; }
        public int? ClaimantId { get; set; }
        public int? eClaimId { get; set; }
        public int? Serial { get; set; }
        public List<int> Docs { get; set; }
    }
    public class EmailInput
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool isApproval { get; set; }
        public int? approvalID { get; set; }
    }
    public class ClaimSearchobj
    {
        public int PolicyId { get; set; }
        public int VehicleId { get; set; }
        public int? Id { get; set; }
        public int? ClaimantId { get; set; }
        public int? SurvoyerId { get; set; }
        public int? RecoveryId { get; set; }
        public int? ApprovalId { get; set; }
        public long? ClaimId { get; set; }
        public int? eClaimId { get; set; }
    }

    public class SearchObj
    {
        public string? claimno { get; set; }
        public string? nationalid { get; set; }
        public string? mobile { get; set; }
        public string? chassis { get; set; }
        public string? policy { get; set; }
        public int? Branch { get; set; }
        public DateTime? RegisteredFrom { get; set; }
        public DateTime? RegisteredTo { get; set; }


    }

    public class docs
    {
        public int DocId { get; set; }
        public int TransId { get; set; }
    }

    public class ApprovalStatus
    {
        public int? Id { get; set; }
        public bool? Status { get; set; }
        public string? Reason { get; set; }
    }


}
