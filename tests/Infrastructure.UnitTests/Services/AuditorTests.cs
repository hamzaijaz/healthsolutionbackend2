using System.Threading.Tasks;
using CapitalRaising.Audit.Contracts;
using CapitalRaising.RightsIssues.Service.Application.Common.Interfaces;
using Models = CapitalRaising.RightsIssues.Service.Application.Common.Models;
using CapitalRaising.RightsIssues.Service.Infrastructure.Services;
using Moq;
using Xunit;


namespace CapitalRaising.RightsIssues.Service.Infrastructure.UnitTests.Services
{
    public class AuditorTests
    {
        [Fact]
        public async Task AddAuditMessage_ShouldCallSenderSendAsync()
        {
            // arrange
            var callContext = new Mock<ICallContext>();
            var factory = new Mock<IBusEndpointFactory>();
            var sender = new Mock<IBusEndpoint>();
            factory.Setup(x => x.Create(AuditCommand.QueueName)).Returns(sender.Object);

            var auditor = new Auditor(callContext.Object, factory.Object);
            var auditEntry = new Models.AuditEntry("Issuer", action: "Modify")
            {
                ActionTarget = new Application.Common.Models.ActionTarget
                {
                    EntityType = "Customer",
                    EntityKey = "12345678"
                },
                Customer = new Application.Common.Models.Customer
                {
                    CustomerID = "12345"
                }
            };
            // act
            await auditor.AddAsync(new Models.Audit(Models.AuditOutcome.Success, auditEntry));

            // assert
            sender.Verify(x => x.SendAsync<AuditCommand>(It.IsAny<AuditCommand>()), Times.Once);
        }
    }
}