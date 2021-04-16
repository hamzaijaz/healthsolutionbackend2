using System.Linq;

namespace CapitalRaising.RightsIssues.Service.Application.Common.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Adds a check digit to the given reference.
        /// </summary>
        /// <param name="referenceNumber">The reference number</param>
        /// <param name="rule">Check digit rule</param>
        /// <returns></returns>
        public static string AddCheckDigit(this string referenceNumber, CheckDigitRule rule)
        {
            if (string.IsNullOrWhiteSpace(referenceNumber))
            {
                throw new System.ArgumentException($"'{nameof(referenceNumber)}' cannot be null or whitespace", nameof(referenceNumber));
            }
            if(referenceNumber.Any(c => !char.IsDigit(c)))
            {
                throw new System.ArgumentException($"'{nameof(referenceNumber)}' must only contain digits.", nameof(referenceNumber));
            }

            int checkDigit=0;
            if(rule == CheckDigitRule.MOD10v5)
            {
                int total = 0;
                for (int i = 0; i < referenceNumber.Length; i++)
                {
                    total += referenceNumber[i] * (i + 1);
                }
                checkDigit = total % 10;
            }
            else if(rule == CheckDigitRule.MOD11v3)
            {
                int weight = 2, multiplier=0;

                for (int i = referenceNumber.Length - 1; i >= 0; i--)
                {
                    multiplier += (referenceNumber[i] - '0') * weight;
                    if (++weight > 7) weight = 2;
                }

                checkDigit = 11 - (multiplier % 11);
                if (checkDigit > 9)
                    checkDigit -= 10;
            }

            return referenceNumber + checkDigit.ToString();
        }
    }
    /// <summary>
    /// Specifies the check digit rule.
    /// </summary>
    public enum CheckDigitRule
    {
        ///<summary>
        /// Modulus 10 v5
        /// </summary>
        MOD10v5,
        ///<summary>
        /// Modulus 11
        /// </summary>
        MOD11v3 
    }
}