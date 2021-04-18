using FluentValidation;
using MyHealthSolution.Service.Application.Patients.Commands.UpdatePatient;

namespace MyHealthSolution.Service.Application.Patients.Commands.CreatePatient
{
    public class UpdatePatientCommandValidator : AbstractValidator<UpdatePatientCommand>
    {
        public UpdatePatientCommandValidator()
        {
            RuleFor(v => v.FirstName)
                .NotNull().WithMessage("FirstName must not be null")
                .NotEmpty().WithMessage("FirstName must not be empty");
            RuleFor(v => v.LastName)
                .NotNull().WithMessage("LastName must not be null")
                .NotEmpty().WithMessage("LastName must not be empty");
            RuleFor(v => v.DateOfBirth)
                .NotNull().WithMessage("DateOfBirth must not be null")
                .NotEmpty().WithMessage("DateOfBirth must not be empty");
            RuleFor(v => v.Gender)
                .NotNull().WithMessage("Gender must not be null")
                .NotEmpty().WithMessage("Gender must not be empty");
            RuleFor(v => v.HealthCoverType)
                .NotNull().WithMessage("HealthCoverType must not be null")
                .NotEmpty().WithMessage("HealthCoverType must not be empty");
            RuleFor(v => v.Postcode)
                .NotNull().WithMessage("Postcode must not be null")
                .NotEmpty().WithMessage("Postcode must not be empty");
            RuleFor(v => v.StreetAddress)
                .NotNull().WithMessage("StreetAddress must not be null")
                .NotEmpty().WithMessage("StreetAddress must not be empty");
            RuleFor(v => v.Suburb)
                .NotNull().WithMessage("Suburb must not be null")
                .NotEmpty().WithMessage("Suburb must not be empty");
            RuleFor(v => v.PolicyNumber)
                .NotNull().WithMessage("PolicyNumber must not be null")
                .NotEmpty().WithMessage("PolicyNumber must not be empty");
            RuleFor(v => v.PatientKey)
                .NotNull().WithMessage("PatientKey must not be null")
                .NotEmpty().WithMessage("PatientKey must not be empty");
        }
    }
}
