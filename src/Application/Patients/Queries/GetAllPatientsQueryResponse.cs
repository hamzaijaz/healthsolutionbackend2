using MyHealthSolution.Service.Application.Patients.Models;
using System.Collections.Generic;

namespace MyHealthSolution.Service.Application.Patients.Queries
{
    public class GetAllPatientsQueryResponse
    {
        public List<PatientDto> Patients { get; set; }
    }
}
