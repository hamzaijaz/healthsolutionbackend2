using FluentValidation;

namespace MyHealthSolution.Service.Application.Patients.Queries
{
    public class GetPatientQueryValidator : AbstractValidator<GetPatientQuery>
    {
        public GetPatientQueryValidator()
        {
            RuleFor(v => v.PatientKey)
                .NotNull().WithMessage("PatientKey must not be null")
                .NotEmpty().WithMessage("PatientKey must not be empty");
        }
    }
}
