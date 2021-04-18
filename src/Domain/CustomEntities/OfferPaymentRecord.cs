using System;

namespace MyHealthSolution.Service.Domain.CustomEntities
{
    public class OfferPaymentRecord
    {
        public string HolderId { get; set; }
        public string FullName { get; set; }
        public string Postcode { get; set; }
        public string CountryCode { get; set; }
        public string EmailAddress { get; set; }
        public string MobileNumber { get; set; }
        public string BankCode { get; set; }
        public string BankAccountNumber { get; set; }
        public string CustodianName { get; set; }
        public string CustodianInvestorId { get; set; }
        public decimal TotalAmountReceived { get; set; }
        public string PaymentReference { get; set; }
        public DateTime? DatePaymentReceived { get; set; }
        public string PaymentDescription { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentFailureReason { get; set; }
    }
}
