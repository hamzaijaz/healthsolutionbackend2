using System;
using System.Collections.Generic;

namespace CapitalRaising.RightsIssues.Service.Application.Common.Interfaces
{
    public interface ICallContext
    {
        Guid CorrelationId
        {
            get;
            set;
        }

        string FunctionName
        {
            get;
            set;
        }
        string AuthenticationType
        {
            get;
            set;
        }

        string UserName
        {
            get;
            set;
        }
        string OriginatingUsername
        {
            get;
            set;
        }
        string OriginatingUserId
        {
            get;
            set;
        }
        string IPAddress
        {
            get;
            set;
        }
        string OriginalSubscriptionKey
        {
            get;
            set;
        }

        Guid? IdempotencyKey
        {
            get;
            set;
        }

        IDictionary<string, string> AdditionalProperties
        {
            get;
        }
    }
}