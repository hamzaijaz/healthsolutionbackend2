using System.Threading.Tasks;

namespace CapitalRaising.RightsIssues.Service.Application.Common.Interfaces
{
    public interface ICaptchaVerifier
    {
        Task<bool> VerifyCaptcha(string recaptchaResponse);
    }
}
