using System;
using System.Threading.Tasks;
using CapitalRaising.RightsIssues.Service.Application.Common.Exceptions;
using MediatR;
using Xunit.Abstractions;

namespace Application.IntegrationTests
{
    public abstract class TestBase
    {
        public readonly ITestOutputHelper Output;

        protected TestBase(ITestOutputHelper testOutput)
        {
            this.Output = testOutput;
            TestSetUp().Wait();
        }
        public async Task TestSetUp()
        {
            await Testing.ResetState();
        }
    }
}