using System;

namespace CapitalRaising.RightsIssues.Service.Application.Common.Models
{
    public class AllocationConfirmEmailCorrespondence
    {
        public IssuerDto Issuer { get; set; }

        public EventDto Event { get; set; }

        public string CorrespondenceType { get; set; }

        public AllocationResultDto[] Allocations { get; set; }
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
            public string EventType { get; set; }
            public decimal PricePerShare { get; set; }
            public string CurrencyCode { get; set; }
        }

        public class AllocationResultDto
        {
            public string FullName { get; set; }
            public string EmailAddress { get; set; }
            public string CountryCode { get; set; }
            public string HolderId { get; set; }
            public decimal TotalInvestmentAmount { get; set; }
            public long EntitledSharesAllocated { get; set; }
            public long AdditionalSharesAllocated { get; set; }
            public decimal AmountAllocated { get; set; }
            public decimal AmountRefunded { get; set; }
            //BusinessNumber
            public string ABN { get; set; }
            public string BankCode { get; set; }
            public string BankAccountNumber { get; set; }
            public string BankAccountName { get; set; }
        }
    }
}
