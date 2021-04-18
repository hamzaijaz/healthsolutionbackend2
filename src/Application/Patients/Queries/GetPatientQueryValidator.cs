using FluentValidation;

namespace MyHealthSolution.Service.Application.Patients.Queries
{
    public class GetPatientQueryValidator : AbstractValidator<GetPatientQuery>
    {
        public GetPatientQueryValidator()
        {
            RuleFor(v => v.PatientKey)
                .NotEmpty();
        }
    }
}
