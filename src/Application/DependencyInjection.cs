using System.Linq;
using MyHealthSolution.Service.Application.Common.Behaviours;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using MyHealthSolution.Service.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MyHealthSolution.Service.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));

            services.AddAuditEnrichersFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }

        private static IServiceCollection AddAuditEnrichersFromAssembly(this IServiceCollection services, Assembly assembly)
        {
            // Register all class maps in the assembly
            var types = assembly.GetExportedTypes();
            // Find all classes which implements IRequestAuditEnricher<>
            var auditEnrichers = from type in types
                where !type.IsAbstract && !type.IsGenericTypeDefinition
                let interfaces = type.GetInterfaces()
                let genericInterfaces = interfaces.Where(i =>
                    i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestAuditEnricher<>))
                let matchingInterface = genericInterfaces.FirstOrDefault()
                where matchingInterface != null
                select new { Interface = matchingInterface, Type = type };

            foreach (var enricher in auditEnrichers )
            {
                services.AddTransient(enricher.Interface, enricher.Type );
            }
            
            return services;
        }
    }
}
