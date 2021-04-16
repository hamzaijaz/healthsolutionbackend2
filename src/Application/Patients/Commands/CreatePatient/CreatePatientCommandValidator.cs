using FluentValidation;

namespace CapitalRaising.RightsIssues.Service.Application.Patients.Commands.CreatePatient
{
    public class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
    {
        public CreatePatientCommandValidator()
        {
            RuleFor(v => v.FirstName)
                .NotEmpty();
            RuleFor(v => v.LastName)
                .NotEmpty();
            RuleFor(v => v.DateOfBirth)
                .NotEmpty();
            RuleFor(v => v.Gender)
                .NotEmpty();
            RuleFor(v => v.HealthCoverType)
                .NotEmpty();
            RuleFor(v => v.Postcode)
                .NotEmpty();
            RuleFor(v => v.StreetAddress)
                .NotEmpty();
            RuleFor(v => v.Suburb)
                .NotEmpty();
            RuleFor(v => v.PolicyNumber)
                .NotEmpty();
        }
    }
}
