using Ardalis.GuardClauses;
using AutoMapper;
using MyHealthSolution.Service.Application.Common.Exceptions;
using MyHealthSolution.Service.Application.Common.Interfaces;
using MyHealthSolution.Service.Application.Patients.Models;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyHealthSolution.Service.Application.Patients.Commands.CreatePatient
{
    public class CreatePatientCommand : IRequest<CreatePatientResponse>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string StreetAddress { get; set; }
        public string Suburb { get; set; }
        public string Postcode { get; set; }
        public string HealthCoverType { get; set; }
        public string PolicyNumber { get; set; }
        public string RecaptchaResponse { get; set; }

        public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, CreatePatientResponse>
        {
            private readonly IApplicationDbContext _dbContext;
            private readonly IMapper _mapper;
            private readonly ICaptchaVerifier _captchaVerifier;

            public CreatePatientCommandHandler(IApplicationDbContext dbContext,
                                                IMapper mapper,
                                                ICaptchaVerifier captchaVerifier)
            {
                Guard.Against.Null(dbContext, nameof(dbContext));
                Guard.Against.Null(mapper, nameof(mapper));
                Guard.Against.Null(captchaVerifier, nameof(captchaVerifier));

                _dbContext = dbContext;
                _mapper = mapper;
                _captchaVerifier = captchaVerifier;
            }

            public async Task<CreatePatientResponse> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
            {
                //Validating reCaptcha
                var captchaPassed = await _captchaVerifier.VerifyCaptcha(request.RecaptchaResponse);
                if (!captchaPassed)
                { 
                    throw new BadRequestException("reCAPTCHA verification failed."); 
                }

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