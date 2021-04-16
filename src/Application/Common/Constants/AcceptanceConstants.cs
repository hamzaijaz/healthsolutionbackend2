﻿namespace CapitalRaising.RightsIssues.Service.Application.Common.Constants
{
    public static class AcceptanceConstants
    {
        public const int MaximumAllowedAcceptances = 5;

        public static class Status
        {
            public const string Unpaid = "unpaid";
            public const string Paid = "paid";
            public const string Underpaid = "underpaid";
            public const string Cancelled = "cancelled";
            public const string Failed = "failed";
            public const string Reduced = "reduced";
            public const string Refunded = "refunded";
        }
    }
}