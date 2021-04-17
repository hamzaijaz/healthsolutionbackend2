using AutoMapper;
using CapitalRaising.RightsIssues.Service.Application.Common.Interfaces;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using CapitalRaising.RightsIssues.Service.Application.Common.Exceptions;
using CapitalRaising.RightsIssues.Service.Domain.Entities;
using CapitalRaising.RightsIssues.Service.Application.Patients.Models;

namespace CapitalRaising.RightsIssues.Service.Application.Patients.Queries
{
    public class GetAllPatientsQuery : IRequest<GetAllPatientsQueryResponse>
    {
        public class GetAllPatientsQueryHandler : IRequestHandler<GetAllPatientsQuery, GetAllPatientsQueryResponse>
        {
            private readonly IApplicationDbContext _dbContext;
            private readonly IMapper _mapper;

            public GetAllPatientsQueryHandler(IApplicationDbContext dbContext, IMapper mapper)
            {
                this._mapper = mapper;
                this._dbContext = dbContext;
            }

            public async Task<GetAllPatientsQueryResponse> Handle(GetAllPatientsQuery request, CancellationToken cancellationToken)
            {
                var patients = new List<Patient>();
                patients = await _dbContext.Patients
                                        .AsNoTracking().ToListAsync();


                // TODO: Mapping of the DTO is done manually here rather than using AutoMapper
                // due to the need to map data from two entities. There may be a technique for
                // implementing this with AutoMapper.
                var response = new GetAllPatientsQueryResponse
                {
                    Patient = patients.Select(patient => new PatientDto()
                    {
                        FirstName = patient.FirstName,
                        LastName = patient.LastName,
                        PatientKey = patient.PatientKey,
                        Gender = patient.Gender,
                        DateOfBirth = patient.DateOfBirth,
                        HealthCoverType = patient.HealthCoverType,
                        Postcode = patient.Postcode,
                        PolicyNumber = patient.PolicyNumber,
                        StreetAddress = patient.StreetAddress,
                        Suburb = patient.Suburb
                    }).ToList()
                };
                return response;
            }
        }

    }
}