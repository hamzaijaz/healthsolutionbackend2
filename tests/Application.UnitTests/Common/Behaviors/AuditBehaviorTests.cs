using System;
using System.Threading;
using System.Threading.Tasks;
using CapitalRaising.RightsIssues.Service.Application.Common;
using CapitalRaising.RightsIssues.Service.Application.Common.Audit;
using MediatR;
using Moq;
using Xunit;
using FluentAssertions;
using CapitalRaising.RightsIssues.Service.Application.Common.Interfaces;
using CapitalRaising.RightsIssues.Service.Application.Common.Behaviours;
using CapitalRaising.RightsIssues.Service.Application.Common.Models;

namespace CapitalRaising.RightsIssues.Service.Application.UnitTests.Common.Behaviors
{
    public class AuditBehaviorTests
    {
        private const string eventFullName = "SomeEntity.Create";
        private const string eventSimpleName = "SomeEntity";
        private const string ErrorMessage = "ArgumentException from RI Service";
        private Mock<IAuditor> mockAuditor;
        private Mock<IServiceProvider> mockServiceProvider;
        private Mock<RequestHandlerDelegate<Guid>> mockSuccessHandler;
        private Mock<RequestHandlerDelegate<Guid>> mockFailHandler;

        private static readonly AuditEntry auditEntry = new AuditEntry("Some", "Create");

        public AuditBehaviorTests()
        {
            mockAuditor = new Mock<IAuditor>();
            mockServiceProvider = new Mock<IServiceProvider>();
            mockSuccessHandler = new Mock<RequestHandlerDelegate<Guid>>();
            mockSuccessHandler.Setup(_ => _()).ReturnsAsync(Guid.NewGuid());

            mockFailHandler = new Mock<RequestHandlerDelegate<Guid>>();
            mockFailHandler.Setup(_ => _()).ThrowsAsync(new ArgumentException(ErrorMessage));
        }

        [Fact]
        public async Task WhenAuditableRequest_ShouldCreateAuditWithSuccessOutcome()
        {
            var sut = new RequestAuditBehavior<SomeAuditableCommand, Guid>(mockAuditor.Object,mockServiceProvider.Object);
            
            var result = await sut.Handle(
                                            new SomeAuditableCommand(), 
                                            new CancellationToken(),
                                            mockSuccessHandler.Object);

            mockSuccessHandler.Verify(_ => _(), Times.Once);
            mockAuditor.Verify(x => x.AddAsync(It.Is<CapitalRaising.RightsIssues.Service.Application.Common.Models.Audit>(a => a.Outcome == AuditOutcome.Success
                                                                && a.Entry != null)), Times.Once);
        }

        [Fact]
        public async Task WhenAuditableRequest_ShouldCreateAuditWithSuccessOutcome_WithMatchingEventNames()
        {
            var sut = new RequestAuditBehavior<SomeAuditableCommand, Guid>(mockAuditor.Object,mockServiceProvider.Object);
            
            var result = await sut.Handle(
                new SomeAuditableCommand(), 
                new CancellationToken(),
                mockSuccessHandler.Object);

            mockSuccessHandler.Verify(_ => _(), Times.Once);
            mockAuditor.Verify(x => x.AddAsync(It.Is<CapitalRaising.RightsIssues.Service.Application.Common.Models.Audit>(a => a.Outcome == AuditOutcome.Success
                                                                                                                                 && a.Entry.EventFullName == eventFullName 
                                                                                                                                 && a.Entry.EventSimpleName == eventSimpleName)), Times.Once);
        }

        [Fact]
        public async Task WhenAuditableRequestAndErrorOccurs_ShouldCreateAuditWithFailureOutcome()
        {            
            var sut = new RequestAuditBehavior<SomeAuditableCommand, Guid>(mockAuditor.Object,mockServiceProvider.Object);
            
            var exception = await Record.ExceptionAsync(() => 
                                             sut.Handle(
                                                        new SomeAuditableCommand(), 
                                                        new CancellationToken(),
                                                        mockFailHandler.Object)
            );

            exception.Should().NotBeNull();
            exception.Message.Should().Be(ErrorMessage);

            mockAuditor.Verify(x => x.AddAsync(It.Is<CapitalRaising.RightsIssues.Service.Application.Common.Models.Audit>(a => a.Outcome == AuditOutcome.Failure 
                                                                && a.Reason == ErrorMessage
                                                                && a.Entry != null)), Times.Once);

            mockFailHandler.Verify(_ => _(), Times.Once);
        }

        [Fact]
        public async Task WhenAuditableRequestAndErrorOccurs_ShouldCreateAuditWithFailureOutcome_WithMatchingEventNames()
        {            
            var sut = new RequestAuditBehavior<SomeAuditableCommand, Guid>(mockAuditor.Object,mockServiceProvider.Object);
            
            var exception = await Record.ExceptionAsync(() => 
                sut.Handle(
                    new SomeAuditableCommand(), 
                    new CancellationToken(),
                    mockFailHandler.Object)
            );

            exception.Should().NotBeNull();
            exception.Message.Should().Be(ErrorMessage);

            mockAuditor.Verify(x => x.AddAsync(It.Is<CapitalRaising.RightsIssues.Service.Application.Common.Models.Audit>(a => a.Outcome == AuditOutcome.Failure 
                                                                                                                                 && a.Entry.EventFullName == eventFullName 
                                                                                                                                 && a.Entry.EventSimpleName == eventSimpleName)), Times.Once);

            mockFailHandler.Verify(_ => _(), Times.Once);
        }

        [Fact]
        public async Task WhenNotAnAuditableRequestAndNoErrorOccured_ShouldNotCreateAudit()
        {
            var sut = new RequestAuditBehavior<SomeQuery, Guid>(mockAuditor.Object,mockServiceProvider.Object);
            
            var exception = await Record.ExceptionAsync(() => 
                                             sut.Handle(
                                                        new SomeQuery() , 
                                                        new CancellationToken(),
                                                        mockSuccessHandler.Object)
            );

            exception.Should().BeNull();
            mockAuditor.Verify(x => x.AddAsync(It.IsAny<CapitalRaising.RightsIssues.Service.Application.Common.Models.Audit>()), Times.Never);
            mockSuccessHandler.Verify(_ => _(), Times.Once);
        }


        [Fact]
        public async Task WhenNotAnAuditableRequestAndErrorOccureds_ShouldReturnErrorButNotCreateAudit()
        {
            var sut = new RequestAuditBehavior<SomeQuery, Guid>(mockAuditor.Object,mockServiceProvider.Object);
            
            var exception = await Record.ExceptionAsync(() => 
                                             sut.Handle(
                                                        new SomeQuery() , 
                                                        new CancellationToken(),
                                                        mockFailHandler.Object)
            );

            exception.Should().NotBeNull();
            exception.Message.Should().Be(ErrorMessage);

            mockAuditor.Verify(x => x.AddAsync(It.IsAny<CapitalRaising.RightsIssues.Service.Application.Common.Models.Audit>()), Times.Never);
            mockFailHandler.Verify(_ => _(), Times.Once);
        }

        private class SomeQuery : IRequest<Guid> {}

        [Audit( typeof(SomeEntity),CapitalRaising.RightsIssues.Service.Application.Common.Audit.Action.Create)]
        private class SomeAuditableCommand : IRequest<Guid>
        {
            public SomeEntity SomeEntity {get;set;}
        }

        private class SomeEntity
        {
            public string Name {get;set;}
        }
    }


}