namespace CapitalRaising.RightsIssues.Service.Application.Common.Interfaces
{
    public interface IBusEndpointFactory
    {
        IBusEndpoint Create(string queueOrTopicName);
        IBusEndpoint Create<TPayload>() where TPayload: class;
    }
}