namespace CapitalRaising.RightsIssues.Service.FunctionApp
{
    internal static class RouteConstants
    {
        internal const string Patient = "patients";
        internal const string RightsIssues = "issuers/{issuerkey}/rightsissues";
        internal const string RightsIssue = "issuers/{issuerkey}/rightsissues/{eventKey}";
        internal const string RightsIssueForCustodian = "issuers/{issuerkey}/rightsissues/{eventKey}/custodians/{custodianKey}";
        internal const string Acceptance = "issuers/{issuerkey}/rightsissues/{eventKey}/acceptances/{acceptanceKey}";
    }
}