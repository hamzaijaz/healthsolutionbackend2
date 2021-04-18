using System.Threading.Tasks;

namespace MyHealthSolution.Service.Application.Common.Interfaces
{
    public interface ICaptchaVerifier
    {
        Task<bool> VerifyCaptcha(string recaptchaResponse);
    }
}
