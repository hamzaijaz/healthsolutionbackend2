using MyHealthSolution.Service.Application.Patients.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyHealthSolution.Service.Application.Patients.Queries
{
    public class GetAllPatientsQueryResponse
    {
        public List<PatientDto> Patient { get; set; }
    }
}
