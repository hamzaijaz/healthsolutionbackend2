using MyHealthSolution.Service.Infrastructure.Services;
using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Moq.Protected;
using System.Threading;
using System.Net;
using FluentAssertions;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace MyHealthSolution.Service.Infrastructure.UnitTests.Services
{
    public class CaptchaVerifierTests
    {
        private readonly RecaptchaConfig _recaptchaConfig;

        public CaptchaVerifierTests()
        {
            _recaptchaConfig = new RecaptchaConfig()
            {
                RecaptchaKey = "Secret"
            };
        }

        [Fact]
        public async Task CaptchaVerifier_EnsureTrue_WhenCaptchaPasses()
        {
            //arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
            .Protected()
            // Setup the PROTECTED method to mock
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            // prepare the expected response of the mocked http call
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"success\": true}"),
            })
            .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://test.com/siteverify")
            };

            var jsonSerializer = new JsonSerializer();

            var captchaService = new CaptchaVerifier(httpClient, jsonSerializer, _recaptchaConfig, Mock.Of<ILogger<CaptchaVerifier>>());
            //act
            var result = await captchaService.VerifyCaptcha("TestCaptchaResponse");

            //assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CaptchaVerifier_EnsureFalse_WhenCaptchaFails()
        {
            //arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
            .Protected()
            // Setup the PROTECTED method to mock
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            // prepare the expected response of the mocked http call
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"success\": false}"),
            })
            .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://test.com/siteverify")
            };

            var jsonSerializer = new JsonSerializer();

            var captchaService = new CaptchaVerifier(httpClient, jsonSerializer, _recaptchaConfig, Mock.Of<ILogger<CaptchaVerifier>>());
            //act
            var result = await captchaService.VerifyCaptcha("TestCaptchaResponse");

            //assert
            result.Should().BeFalse();
        }
    }
}