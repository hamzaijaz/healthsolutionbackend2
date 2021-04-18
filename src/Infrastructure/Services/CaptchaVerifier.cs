using MyHealthSolution.Service.Application.Common.Interfaces;
using System;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MyHealthSolution.Service.Infrastructure.Services
{
    public class CaptchaVerifier : ICaptchaVerifier
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializer _jsonSerializer;
        private readonly ILogger<CaptchaVerifier> _log;
        private readonly RecaptchaConfig _recaptchaConfig;

        public CaptchaVerifier(HttpClient httpClient, JsonSerializer jsonSerializer, RecaptchaConfig recaptchaConfig, ILogger<CaptchaVerifier> log)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _log = log;
            _recaptchaConfig = recaptchaConfig;
        }
        public async Task<bool> VerifyCaptcha(string recaptchaResponse)
        {
            var verificationResponse = await _httpClient.GetAsync($"?secret={_recaptchaConfig.RecaptchaKey}&response={recaptchaResponse}");
            var verificationContent = await verificationResponse.Content.ReadAsStringAsync();

            if (!verificationResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Error while sending request to reCAPTCHA service. {verificationContent}");
            }

            // Not bothering to create a model for the verification response object.
            dynamic verificationResult = JsonConvert.DeserializeObject(verificationContent);

            if (verificationResult?.success == false)
            {
                _log.LogInformation($"reCAPTCHA verification failed.");
                return false;
            }
            return true;
        }
    }
}
