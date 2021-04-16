using System;

namespace CapitalRaising.Administration.Service.Application.SettlementBankAccounts.Messages
{
    public class SettlementBankAccountChangedEvent
    {
        public const string EndpointName = "capitalraising.administration.messages.settlementbankaccountchangedevent";

        public Guid IssuerKey { get; set; }
        public Guid EventKey { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountName { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public string CountryCode { get; set; }
        public string BillerCode { get; set; }
    }
}