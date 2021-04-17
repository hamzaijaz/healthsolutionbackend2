using CapitalRaising.RightsIssues.Service.Application.Patients.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapitalRaising.RightsIssues.Service.Application.Patients.Queries
{
    public class GetAllPatientsQueryResponse
    {
        public List<PatientDto> Patient { get; set; }
    }
}
