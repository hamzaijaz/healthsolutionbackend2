using AutoMapper;
using MyHealthSolution.Service.Application.Common.Interfaces;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MyHealthSolution.Service.Domain.Entities;
using MyHealthSolution.Service.Application.Patients.Models;

namespace MyHealthSolution.Service.Application.Patients.Queries
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

                var response = new GetAllPatientsQueryResponse
                {
                    Patients = patients.Select(patient => new PatientDto()
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