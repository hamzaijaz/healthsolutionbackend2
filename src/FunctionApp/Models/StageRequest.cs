using System;

namespace  MyHealthSolution.Service.FunctionApp.Models
{
    public class StageRequest
    {
        public Guid IssuerKey {get; set;}
        public Guid EventKey {get; set;}
    }
}