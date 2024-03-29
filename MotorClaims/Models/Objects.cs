﻿namespace MotorClaims.Models
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

    public class ClaimSearchobj
    {
        public int PolicyId { get; set; }
        public int VehicleId { get; set; }
        public int? Id { get; set; }
        public int? ClaimantId { get; set; }
        public int? SurvoyerId { get; set; }
        public long? ClaimId { get; set; }
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
}
