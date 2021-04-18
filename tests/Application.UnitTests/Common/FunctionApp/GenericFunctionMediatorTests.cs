using System;
using System.Threading;
using System.Threading.Tasks;
using MyHealthSolution.Service.FunctionApp;
using MyHealthSolution.Service.Infrastructure.Context;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Application.UnitTests.Common.FunctionApp
{
    public class GenericFunctionMediatorTests
    {
        public GenericFunctionMediatorTests()
        {
        }

        [Fact]
        public async Task ExecuteAsync_EnsureDiagnosticHeadersFlowToCall_RequestOneWay()
        {
            // arrange
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new Unit());

            var invocationId = Guid.NewGuid();
            var functionName = nameof(ExecuteAsync_EnsureDiagnosticHeadersFlowToCall_RequestOneWay);
            var callContext = new MutableCallContext();
            
            var sut = new GenericFunctionMediator(mockMediator.Object, callContext);

            // act
            await sut.ExecuteAsync<Request>(invocationId, functionName, new Request());
            
            // assert
            mockMediator.Verify(_ => _.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()));
            callContext.CorrelationId.Should().Be(invocationId);
            callContext.FunctionName.Should().Be(functionName);
        }

        [Fact]
        public async Task ExecuteAsync_EnsureDiagnosticHeadersFlowToCall_RequestResponse()
        {
            // arrange
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<Request>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new Unit());

            var invocationId = Guid.NewGuid();
            var functionName = nameof(ExecuteAsync_EnsureDiagnosticHeadersFlowToCall_RequestResponse);
            var callContext = new MutableCallContext();
            
            var sut = new GenericFunctionMediator(mockMediator.Object, callContext);

            // act
            var response = await sut.ExecuteAsync<RequestWithResponse, Response>(invocationId, functionName, new RequestWithResponse());
            
            // assert
            mockMediator.Verify(_ => _.Send(It.IsAny<RequestWithResponse>(), It.IsAny<CancellationToken>()));
            callContext.CorrelationId.Should().Be(invocationId);
            callContext.FunctionName.Should().Be(functionName);
        }

        private class Request : IRequest
        {
        }

        private class Response
        {
        }

        private class RequestWithResponse : IRequest<Response>
        {
        }
    }
}