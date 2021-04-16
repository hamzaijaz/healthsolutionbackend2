using System;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using CapitalRaising.RightsIssues.Service.Application.Common.Interfaces;
using Models = CapitalRaising.RightsIssues.Service.Application.Common.Models;
using CapitalRaising.Audit.Contracts;


namespace CapitalRaising.RightsIssues.Service.Infrastructure.Services
{
    public class Auditor : IAuditor
    {
        private readonly ICallContext callContext;
        private readonly IBusEndpoint bus;

        public Auditor(ICallContext callContext, IBusEndpointFactory busFactory)
        {
            Guard.Against.Null(callContext, nameof(callContext));
            Guard.Against.Null(busFactory, nameof(busFactory));

            this.callContext = callContext;
            
            this.bus = busFactory.Create(AuditCommand.QueueName);
        }

        public async Task AddAsync(Models.Audit audit)
        {
            var cmd = new AuditCommand
            {
                ActionStatus = audit.Outcome.ToString(),
                FailureReason = audit.Reason,
                CorrelationId = callContext.CorrelationId.ToString(),
                Customer = new Customer
                {
                    CustomerID = audit.Entry.Customer?.CustomerID,
                    CustomerCode = audit.Entry.Customer?.CustomerCode,
                    CustomerContext = audit.Entry.Customer?.CustomerContext
                },
                EventSimpleName = audit.Entry.EventSimpleName,
                EventFullName = audit.Entry.EventFullName,
                EventDateTimeUTC = audit.Entry.DateOccuredUtc,
                ActionTarget = new ActionTarget
                {
                    EntityType = audit.Entry.ActionTarget?.EntityType,
                    EntityKey1 = audit.Entry.ActionTarget?.EntityKey,
                    EntityKey2 = audit.Entry.ActionTarget?.EntityKey2,
                },
                CustomData = audit.Entry.CustomData,
                Actor = new Actor
                {
                    UserID = callContext.OriginatingUserId,
                    UserName = callContext.OriginatingUsername,
                    IPAddress = callContext.IPAddress
                },
                ExecutionProcess = new ExecutionProcess
                {
                    UserName = Environment.UserName,
                    ExecutingApplication = new CapitalRaising.Audit.Contracts.Application
                    {
                        ApplicationName = audit.Entry.ExecutingApplication,
                        ModuleName = callContext.FunctionName
                    }
                }
            };
            // send message command to integration bus
            await this.bus.SendAsync(cmd);
        }
    }

}