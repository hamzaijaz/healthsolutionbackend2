using AutoMapper;
using MyHealthSolution.Service.Application.Common.Mappings;
using MyHealthSolution.Service.Domain.Entities;
using System;
using Entities = MyHealthSolution.Service.Domain.Entities;


namespace MyHealthSolution.Service.Application.Patients.Models
{
    public class PatientDto : IMapFrom<Entities.Patient>
    {
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

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Patient, PatientDto>();
        }
    }
}
