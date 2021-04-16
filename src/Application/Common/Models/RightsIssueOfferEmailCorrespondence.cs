using System;

namespace CapitalRaising.RightsIssues.Service.Application.Common.Models
{
    public class RightsIssueOfferEmailCorrespondence
    {
        public IssuerDto Issuer { get; set; }

        public EventDto Event { get; set; }

        public string CorrespondenceType { get; set; }

        public ShareholderDto[] Shareholders { get; set; }

        public bool IsReminder  { get; set; }

        public string CurrencySymbol { get; set; }


        public class IssuerDto
        {
            public string CompanyName { get; set; }
            public string TickerCode { get; set; }
            public Guid IssuerKey { get; set; }
            public string ContactName { get; set; }
            public string ContactPositionTitle { get; set; }
        }

        public class EventDto
        {
            public Guid EventKey { get; set; }
            public string EventName { get; set; }
            public string EventType { get; set; }  // (SharePurchasePlan or RightsIssue)
            public decimal PricePerShare { get; set; }
            public string CurrencyCode { get; set; }
            public decimal AmountToBeRaised { get; set; }
            public DateTime AnnouncementDate { get; set; }
            public DateTime RecordDate { get; set; }
            public DateTime OfferOpenDate { get; set; }
            public DateTime OfferCloseDate { get; set; }
            public DateTime IssueDate { get; set; }
            public int NewSharesRatio { get; set; }
            public int ForSharesHeldRatio { get; set; }
            public DateTime ExDate { get; set; }
        }

        public class ShareholderDto
        {
            public Guid OfferKey { get; set; }
            public string EmailAddress { get; set; }
            public string CountryCode { get; set; }
            public string HolderId { get; set; }
            public string ProtectedHolderId { get; set; }
            public string FullName { get; set; }

        }
    }
}
