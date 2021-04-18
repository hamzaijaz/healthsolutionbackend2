using System;
using AutoMapper;
using MyHealthSolution.Service.Application.Patients.Models;
using MyHealthSolution.Service.Application.Patients.Queries;
using MyHealthSolution.Service.Domain.Entities;

namespace MyHealthSolution.Service.FunctionApp.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<PatientDto, Patient>()
                .ForMember(x => x.FirstName, s => s.MapFrom(source => source.FirstName))
                .ForMember(x => x.LastName, s => s.MapFrom(source => source.LastName))
                .ForMember(x => x.Gender, s => s.MapFrom(source => source.Gender))
                .ForMember(x => x.DateOfBirth, s => s.MapFrom(source => source.DateOfBirth))
                .ForMember(x => x.PatientKey, s => s.MapFrom(source => source.PatientKey))
                .ForMember(x => x.HealthCoverType, s => s.MapFrom(source => source.HealthCoverType))
                .ForMember(x => x.PolicyNumber, s => s.MapFrom(source => source.PolicyNumber))
                .ForMember(x => x.Postcode, s => s.MapFrom(source => source.Postcode))
                .ForMember(x => x.PatientId, s => s.Ignore())
                .ForMember(x => x.LastUpdatedAtUtc, s => s.Ignore())
                .ForMember(x => x.StreetAddress, s => s.MapFrom(source => source.StreetAddress))
                .ForMember(x => x.Suburb, s => s.MapFrom(source => source.Suburb));
        }
    }
}