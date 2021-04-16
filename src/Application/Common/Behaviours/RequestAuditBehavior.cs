using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CapitalRaising.RightsIssues.Service.Application.Common.Audit;
using CapitalRaising.RightsIssues.Service.Application.Common.Interfaces;
using CapitalRaising.RightsIssues.Service.Application.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Action = CapitalRaising.RightsIssues.Service.Application.Common.Audit.Action;

namespace CapitalRaising.RightsIssues.Service.Application.Common.Behaviours
{
    public class RequestAuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IAuditor auditor;
        private readonly IServiceProvider serviceProvider;

        public RequestAuditBehavior(IAuditor auditor, IServiceProvider serviceProvider)
        {
            this.auditor = auditor;
            this.serviceProvider = serviceProvider;
        }

        private bool IsAuditable()
        {
            return (typeof(TRequest).GetCustomAttribute<AuditAttribute>()) != null;
        }

        private AuditEntry CreatAuditEntry()
        {
            // Get contents from the attribute.
            var attribute = typeof(TRequest).GetCustomAttribute<AuditAttribute>();
            if (attribute == null) throw new InvalidOperationException($"Expected AuditAttribute on {typeof(TRequest).Name}, but it is not found." );

            return new AuditEntry(name: typeof(TRequest).Name, action: attribute.Action.ToString())
            {
                EventSimpleName = attribute.EntityType,
                EventFullName = string.Concat(attribute.EntityType, "." ,attribute.Action.ToString()),
            };

        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {

            Models.AuditEntry entry = null;
            bool isAuditable = IsAuditable();

            if (isAuditable)
            {
                entry = CreatAuditEntry();
                var instance = this.serviceProvider.GetService<IRequestAuditEnricher<TRequest>>();
                if (instance != null)
                {
                    await instance.EnrichAudit(entry, request);
                }
            }
            //Note try catch should be outside condition as next() must be called within Behavior
            try
            {
                var response = await next();

                if (isAuditable)
                {
                    // record successful audit
                    await this.auditor.AddAsync(
                        new Models.Audit(outcome: AuditOutcome.Success, entry)
                    );
                }

                return response;
            }
            catch (System.Exception ex)
            {
                // Error
                if (isAuditable)
                {
                    string exceptionMessage = ex.GetType().Name + " from RI Service";

                    // record audit failure
                    await this.auditor.AddAsync(
                        new Models.Audit(outcome: AuditOutcome.Failure, entry, reason: exceptionMessage)
                    );
                }
                throw;

            }

        }
    }
}