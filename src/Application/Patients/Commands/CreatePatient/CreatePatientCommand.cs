using Ardalis.GuardClauses;
using AutoMapper;
using MyHealthSolution.Service.Application.Common.Exceptions;
using MyHealthSolution.Service.Application.Common.Interfaces;
using MyHealthSolution.Service.Application.Patients.Models;
using MyHealthSolution.Service.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyHealthSolution.Service.Application.Patients.Commands.CreatePatient
{
    public class CreatePatientCommand : IRequest<CreatePatientResponse>
    {
        //public PatientDto Patient { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string StreetAddress { get; set; }
        public string Suburb { get; set; }
        public string Postcode { get; set; }
        public string HealthCoverType { get; set; }
        public string PolicyNumber { get; set; }

        public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, CreatePatientResponse>
        {
            private readonly IApplicationDbContext _dbContext;
            private readonly IMapper _mapper;

            public CreatePatientCommandHandler(IApplicationDbContext dbContext,
                                                IMapper mapper)
            {
                Guard.Against.Null(dbContext, nameof(dbContext));
                Guard.Against.Null(mapper, nameof(mapper));

                _dbContext = dbContext;
                _mapper = mapper;
            }

            public async Task<CreatePatientResponse> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
            {
                var patient = new Domain.Entities.Patient
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PatientKey = Guid.NewGuid(),
                    Gender = request.Gender,
                    DateOfBirth = request.DateOfBirth,
                    HealthCoverType = request.HealthCoverType,
                    Postcode = request.Postcode,
                    PolicyNumber = request.PolicyNumber,
                    StreetAddress = request.StreetAddress,
                    Suburb = request.Suburb
                };
                _dbContext.Patients.Add(patient);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return new CreatePatientResponse
                {
                    Patient = _mapper.Map<PatientDto>(patient)
                };
            }
        }

    }
}