using AutoMapper;
using Microsoft.Azure.WebJobs;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using MyHealthSolution.Service.Application.Patients.Queries;
using MyHealthSolution.Service.Application.Patients.Commands.CreatePatient;

namespace MyHealthSolution.Service.FunctionApp.HttpTriggers
{
    [ExcludeFromCodeCoverage] // API tests are expected to cover this area
    public class PatientFunctions
    {
        private readonly IHttpFunctionMediator _httpFunctionMediator;
        private readonly IMapper _mapper;

        public PatientFunctions(IHttpFunctionMediator httpFunctionMediator,
                                    IMapper mapper)
        {
            _mapper = mapper;
            _httpFunctionMediator = httpFunctionMediator;
        }

        [FunctionName("CreatePatient")]
        public async Task<IActionResult> CreatePatient(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = RouteConstants.Patient)] CreatePatientCommand request,
            HttpRequest req,
            Microsoft.Azure.WebJobs.ExecutionContext context)
        {
            return await _httpFunctionMediator.ExecuteAsync<CreatePatientCommand, CreatePatientResponse>(
                context.InvocationId,
                context.FunctionName,
                req,
                request,
                (r) => new CreatedObjectResult("patients", r.Patient.PatientKey.ToString(), r.Patient).ToTask());
        }

        [FunctionName("GetPatient")]
        public async Task<IActionResult> GetPatient(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = RouteConstants.Patient + "/{patientkey}")] GetPatientQuery queryArg,
            HttpRequest req,
            Microsoft.Azure.WebJobs.ExecutionContext context)
        {
            return await _httpFunctionMediator.ExecuteAsync<GetPatientQuery, GetPatientQueryResponse>(
                context.InvocationId,
                context.FunctionName,
                req,
                queryArg,
                (r) => new ObjectResult((r.Patient)).ToTask());
        }

        [FunctionName("GetAllPatients")]
        public async Task<IActionResult> GetAllPatients(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = RouteConstants.Patient)] GetAllPatientsQuery queryArg,
            HttpRequest req,
            Microsoft.Azure.WebJobs.ExecutionContext context)
        {
            return await _httpFunctionMediator.ExecuteAsync<GetAllPatientsQuery, GetAllPatientsQueryResponse>(
                context.InvocationId,
                context.FunctionName,
                req,
                queryArg,
                (r) => new ObjectResult((r.Patient)).ToTask());
        }
    }
}
