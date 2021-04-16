using System;
namespace CapitalRaising.RightsIssues.Service.Domain.CustomEntities
{
    public class Statistic
    {
        public decimal TargetAmount { get; set; }
        public decimal TotalElectedAmount { get; set; }
        public int DaysToGo { get; set; }
        public decimal TotalPaymentReceived { get; set; }
        public decimal TotalPaymentOutstanding { get; set; }
        public decimal TotalEntitledAmountAccepted { get; set; }
        public decimal PercentageEntitledAmountAccepted { get; set; }
        public decimal TotalAdditionalAmountAccepted { get; set; }
        public decimal percentageAdditionalAmountAccepted { get; set; }
        public decimal AmountToRefund { get; set; }
        public decimal AmountRaised { get; set; }
        public decimal PercentageTargetRaised { get; set; }
        public decimal TotalUnmatchedPayments { get; set; }
        public int TotalUnmatchedPaymentCount { get; set; }
        public decimal TotalAmountAllocated { get; set; }
        
    }
}
