using System;
using AutoMapper;
using MyHealthSolution.Service.Application.Patients.Models;
//using MyHealthSolution.Service.Application.MyHealthSolution.Commands.CreateRightsIssue;
//using MyHealthSolution.Service.Application.MyHealthSolution.Commands.UpdateRightsIssue;
//using MyHealthSolution.Service.Application.MyHealthSolution.Models;
//using MyHealthSolution.Service.Application.MyHealthSolution.Queries;
using MyHealthSolution.Service.Domain.Entities;
using MyHealthSolution.Service.FunctionApp.Models;

namespace MyHealthSolution.Service.FunctionApp.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //CreateMap<RightsIssueRequest, RightsIssueDto>()
            //    .ForMember(x => x.LastUpdatedAtUtc, s => s.Ignore())
            //    .ForMember(x => x.EventKey, s => s.MapFrom(source => source.EventKey.GetValueOrDefault(Guid.NewGuid())));
            //CreateMap<RightsIssueRequest, CreateRightsIssueCommand>()
            //    .ForMember(x => x.RightsIssue, s => s.MapFrom(source => source));

            //CreateMap<RightsIssueRequest, UpdateRightsIssueCommand>()
            //    .ForMember(x => x.RightsIssue, s => s.MapFrom(source => source));

            //CreateMap<PatientDto, Patient>()
            //    .ForAllOtherMembers(x => x.MapAtRuntime());

            //CreateMap<Patient, PatientDto>()
            //    .ForAllOtherMembers(x => x.MapAtRuntime());

        }
    }
}