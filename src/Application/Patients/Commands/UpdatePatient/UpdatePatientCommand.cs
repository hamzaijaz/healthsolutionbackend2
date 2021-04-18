using Ardalis.GuardClauses;
using AutoMapper;
using MyHealthSolution.Service.Application.Common.Exceptions;
using MyHealthSolution.Service.Application.Common.Interfaces;
using MyHealthSolution.Service.Application.Patients.Models;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MyHealthSolution.Service.Domain.Entities;

namespace MyHealthSolution.Service.Application.Patients.Commands.UpdatePatient
{
    public class UpdatePatientCommand : IRequest<UpdatePatientResponse>
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

        public class UpdatePatientCommandHandler : IRequestHandler<UpdatePatientCommand, UpdatePatientResponse>
        {
            private readonly IApplicationDbContext _dbContext;
            private readonly IMapper _mapper;

            public UpdatePatientCommandHandler(IApplicationDbContext dbContext, IMapper mapper)
            {
                Guard.Against.Null(dbContext, nameof(dbContext));
                Guard.Against.Null(mapper, nameof(mapper));

                _dbContext = dbContext;
                _mapper = mapper;
            }

            public async Task<UpdatePatientResponse> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
            {
                var existingPatient = await _dbContext.Patients
                                        .Where(i => i.PatientKey == request.PatientKey)
                                        .FirstOrDefaultAsync();

                if (existingPatient == null)
                {
                    throw new NotFoundException(nameof(Patient), request.PatientKey);
                }
                
                existingPatient.FirstName = request.FirstName;
                existingPatient.LastName = request.LastName;
                existingPatient.Gender = request.Gender;
                existingPatient.DateOfBirth = request.DateOfBirth;
                existingPatient.HealthCoverType = request.HealthCoverType;
                existingPatient.Postcode = request.Postcode;
                existingPatient.PolicyNumber = request.PolicyNumber;
                existingPatient.StreetAddress = request.StreetAddress;
                existingPatient.Suburb = request.Suburb;

                await _dbContext.SaveChangesAsync(cancellationToken);

                return new UpdatePatientResponse
                {
                    Patient = _mapper.Map<PatientDto>(existingPatient)
                };
            }
        }
    }
}