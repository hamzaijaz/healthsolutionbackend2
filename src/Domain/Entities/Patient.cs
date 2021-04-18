using System;
using System.Collections.Generic;

namespace MyHealthSolution.Service.Domain.Entities
{
    public partial class Patient
    {
        public int PatientId { get; set; }
        public Guid PatientKey { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string StreetAddress { get; set; }
        public string Suburb { get; set; }
        public string Postcode { get; set; }
        public string HealthCoverType { get; set; }
        public string PolicyNumber { get; set; }
        public DateTime LastUpdatedAtUtc { get; set; }
    }
}
