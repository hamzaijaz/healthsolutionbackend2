using System;

namespace CapitalRaising.RightsIssues.Service.FunctionApp.Models
{
    public class RightsIssueRequest
    {
        public Guid? EventKey { get; set; }
        public Guid IssuerKey { get; set; }
        public string Name { get; set; }
        public decimal PricePerShare { get; set; }
        public string CurrencyCode { get; set; }
        public int NewSharesRatio { get; set; }
        public int ForSharesHeldRatio { get; set; }
        public decimal AmountToBeRaised { get; set; }
        public long NumberOfSharesToBeIssued { get; set; }
        public bool AcceptedTermsAndConditions { get; set; }
        public DateTime AnnouncementDate { get; set; }
        public DateTime ExDate { get; set; }
        public DateTime RecordDate { get; set; }
        public DateTime OfferOpenDate { get; set; }
        public DateTime OfferCloseDate { get; set; }
        public DateTime IssueDate { get; set; }
        public bool AllowOverSubscription { get; set; }
        public string RightsIssueType {get;set;}
        public bool AllowBpay { get; set; }
        public bool AllowDirectDebit { get; set; }

    }
}