
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using AutoMapper;
using CapitalRaising.RightsIssues.Service.Application;
using CapitalRaising.RightsIssues.Service.Infrastructure;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(CapitalRaising.RightsIssues.Service.FunctionApp.Startup))]

namespace CapitalRaising.RightsIssues.Service.FunctionApp
{
    public class Startup : FunctionsStartup
    {
        [ExcludeFromCodeCoverage]
        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigureServices(builder.Services);
        }

        public IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IHttpFunctionMediator, HttpFunctionMediator>();
            services.AddTransient<IFunctionMediator, GenericFunctionMediator>();
            // NOTE: We must call AddAutoMapper once for the entire application therefore Map Profiles
            // accross all assemblies are loaded here.
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies()
                                    .Where(x => x.GetName().Name.StartsWith("CapitalRaising.RightsIssues.Service")));

            services.AddApplication();

            // TODO The Azure Function should not have any dependency on Infrastructure other than DI
            // Determine a better technique
            services.AddInfrastructure();

            return services;
        }
    }
}