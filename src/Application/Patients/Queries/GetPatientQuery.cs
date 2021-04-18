using AutoMapper;
using MyHealthSolution.Service.Application.Common.Exceptions;
using MyHealthSolution.Service.Application.Common.Interfaces;
using MyHealthSolution.Service.Domain.Entities;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyHealthSolution.Service.Application.Patients.Models;

namespace MyHealthSolution.Service.Application.Patients.Queries
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

                return new GetPatientQueryResponse
                {
                    Patient = _mapper.Map<PatientDto>(patient)
                };
            }
        }

    }
}