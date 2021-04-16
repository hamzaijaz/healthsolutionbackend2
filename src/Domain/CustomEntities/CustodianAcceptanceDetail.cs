using System;

namespace CapitalRaising.RightsIssues.Service.Domain.CustomEntities
{
    public class CustodianAcceptanceDetail
    {
        public Guid CustodianId { get; set; }
        public string CompanyName { get; set; }
        public string Ticker { get; set; }
        public string EventName { get; set; }
        public decimal PricePerShare { get; set; }
        public string HolderId { get; set; }
        public Guid OfferId { get; set; }
        public long TotalSharesEntitled { get; set; }
        public decimal TotalEntitledAmountToPay { get; set; }
        public string CustodianInvestorId { get; set; }
        public string FullName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Address4 { get; set; }
        public string Address5 { get; set; }
        public string Address6 { get; set; }
        public string Postcode { get; set; }
        public string CountryCode { get; set; }
        public string EmailAddress { get; set; }
        public string MobileNumber { get; set; }
        public long EntitledSharesAccepted { get; set; }
        public long AdditionalSharesAccepted { get; set; }
        public long TotalSharesAccepted { get; set; }
        public decimal TotalAmountAccepted { get; set; }
        public long SharesAllocated { get; set; }
        public decimal AmountToRefund { get; set; }
        public decimal ScaleBackRatio { get; set; }

    }
}
