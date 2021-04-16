using System;
using AutoMapper;
using CapitalRaising.RightsIssues.Service.Application.Patients.Models;
//using CapitalRaising.RightsIssues.Service.Application.RightsIssues.Commands.CreateRightsIssue;
//using CapitalRaising.RightsIssues.Service.Application.RightsIssues.Commands.UpdateRightsIssue;
//using CapitalRaising.RightsIssues.Service.Application.RightsIssues.Models;
//using CapitalRaising.RightsIssues.Service.Application.RightsIssues.Queries;
using CapitalRaising.RightsIssues.Service.Domain.Entities;
using CapitalRaising.RightsIssues.Service.FunctionApp.Models;

namespace CapitalRaising.RightsIssues.Service.FunctionApp.Mappings
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

            //CreateMap<GetRightsIssuesQueryResponse, RightsIssueResponse>()
            //     .ForMember(x => x.RightsIssues, s => s.MapFrom(source => source.RightsIssues));

        }
    }
}