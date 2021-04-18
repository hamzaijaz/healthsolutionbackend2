namespace MyHealthSolution.Service.Application.Common.Constants
{
    public static class PaymentConstants
    {
        public const string SourceSystem = "RightsIssue";
        public const string EventTypeAbbreviation = " RI";

        public static class StatusReason
        {
            public const string SuccessfullyMatched = "Successfully matched";
            public const string CannotFindMatchingReference = "Can't find matching reference";
        }

        public static class Status
        {
            public const string New = "new";
            public const string Pending = "pending";
            public const string Success = "success";
            public const string Failed = "failed";
            public const string Unmatched = "unmatched";
            public const string Matched = "matched";
        }

        public static class Type
        {
            public const string DirectDebit = "directdebit";
            public const string EFT = "eft";
            public const string Refund = "refund";
            public const string BPAY = "bpay";
        }
    }
}
