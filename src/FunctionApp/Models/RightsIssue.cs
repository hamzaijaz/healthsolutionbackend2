using System;
using MyHealthSolution.Service.FunctionApp.JsonConverters;
using Newtonsoft.Json;

namespace MyHealthSolution.Service.FunctionApp.Models
{
    public class RightsIssue
    {
        public Guid EventKey { get; set; }
        public Guid IssuerKey { get; set; }
        public string Name { get; set; }
        public decimal PricePerShare { get; set; }
        public string CurrencyCode { get; set; }
        public int NewSharesRatio { get; set; }
        public int ForSharesHeldRatio { get; set; }
        public decimal AmountToBeRaised { get; set; }
        public long NumberOfSharesToBeIssued { get; set; }
        public bool AcceptedTermsAndConditions { get; set; }
        
        [JsonConverter(typeof(ShortDateConverter))]
        public DateTime AnnouncementDate { get; set; }

        [JsonConverter(typeof(ShortDateConverter))]
        public DateTime ExDate { get; set; }

        [JsonConverter(typeof(ShortDateConverter))]
        public DateTime RecordDate { get; set; }

        [JsonConverter(typeof(ShortDateConverter))]
        public DateTime OfferOpenDate { get; set; }

        [JsonConverter(typeof(ShortDateConverter))]
        public DateTime OfferCloseDate { get; set; }

        [JsonConverter(typeof(ShortDateConverter))]
        public DateTime IssueDate { get; set; }
        public bool AllowOverSubscription { get; set; }
        public DateTime LastUpdatedAtUtc { get; set; }
        public string RightsIssueType { get; set; }
        public bool AllowBpay { get; set; }
        public bool AllowDirectDebit { get; set; }
    }
}