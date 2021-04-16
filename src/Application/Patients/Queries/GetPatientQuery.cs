using AutoMapper;
using CapitalRaising.RightsIssues.Service.Application.Common.Exceptions;
using CapitalRaising.RightsIssues.Service.Application.Common.Interfaces;
using CapitalRaising.RightsIssues.Service.Domain.Entities;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CapitalRaising.RightsIssues.Service.Application.Patients.Models;

namespace CapitalRaising.RightsIssues.Service.Application.Patients.Queries
{
    public class GetPatientQuery : IRequest<GetPatientQueryResponse>
    {
        public Guid PatientKey { get; set; }

        public class GetPatientQueryHandler : IRequestHandler<GetPatientQuery, GetPatientQueryResponse>
        {
            private readonly IApplicationDbContext _dbContext;
            private readonly IMapper _mapper;

            public GetPatientQueryHandler(IApplicationDbContext dbContext, IMapper mapper)
            {
                this._mapper = mapper;
                this._dbContext = dbContext;
            }

            public async Task<GetPatientQueryResponse> Handle(GetPatientQuery request, CancellationToken cancellationToken)
            {
                var patient = await _dbContext.Patients
                                        .Where(i => i.PatientKey == request.PatientKey)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();

                if (patient == null)
                {
                    throw new NotFoundException(nameof(Patient), request.PatientKey);
                }

                // TODO: Mapping of the DTO is done manually here rather than using AutoMapper
                // due to the need to map data from two entities. There may be a technique for
                // implementing this with AutoMapper.
                var response = new GetPatientQueryResponse
                {
                    Patient = new PatientDto
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
                    }
                };

                return response;
            }
        }

    }
}