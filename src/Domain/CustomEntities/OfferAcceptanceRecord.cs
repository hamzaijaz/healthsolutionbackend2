using System;
using System.Collections.Generic;
using System.Text;

namespace CapitalRaising.RightsIssues.Service.Domain.CustomEntities
{
    public class OfferAcceptanceRecord
    {
        public string HolderId { get; set; }
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
        public string BankCode { get; set; }
        public string BankAccountNumber { get; set; }
        public long RecordDateBalance { get; set; }
        public long TotalSharesEntitled { get; set; }
        public decimal EntitledAmount { get; set; }
        public decimal AcceptanceAmount { get; set; }
        public decimal EntitledAmountPaid { get; set; }
        public decimal AdditionalAmountPaid { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public long TotalSharesAllocated { get; set; }
        public decimal TotalAllocatedAmount { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public decimal ScalebackRatio { get; set; }
    }
}
