using System;
using System.Collections.Generic;
using System.Text;

namespace CapitalRaising.RightsIssues.Service.Application.Common.Models
{
    public class AcceptanceRefundEmailsCorrespondance
    {
        public IssuerDto Issuer { get; set; }

        public EventDto Event { get; set; }

        public string CorrespondenceType { get; set; }

        public List<AcceptanceRefundsDto> AcceptanceRefunds { get; set; } = new List<AcceptanceRefundsDto>();


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
            public string EventType { get; set; }  // (SharePurchasePlan or RightsIssue)
            public string EventName { get; set; }
            public string CurrencyCode { get; set; }
            public string CurrencySymbol { get; set; }
        }

        public class AcceptanceRefundsDto
        {
            public string FullName { get; set; }
            public string EmailAddress { get; set; }
            public string CountryCode { get; set; }
            public string HolderId { get; set; }
            public decimal RefundAmount { get; set; }
            public string ABN { get; set; } //BusinessNumber
            public string BankCode { get; set; }
            public string BankAccountNumber { get; set; }
            public string BankAccountName { get; set; }
        }
    }
}
