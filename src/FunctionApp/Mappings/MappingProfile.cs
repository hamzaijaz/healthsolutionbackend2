using System;
using AutoMapper;
using MyHealthSolution.Service.Application.Patients.Models;
using MyHealthSolution.Service.Domain.Entities;

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
        }
    }
}