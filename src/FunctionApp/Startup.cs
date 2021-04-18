
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using AutoMapper;
using MyHealthSolution.Service.Application;
using MyHealthSolution.Service.Infrastructure;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(MyHealthSolution.Service.FunctionApp.Startup))]

namespace MyHealthSolution.Service.FunctionApp
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
                                    .Where(x => x.GetName().Name.StartsWith("MyHealthSolution.Service")));

            services.AddApplication();

            // TODO The Azure Function should not have any dependency on Infrastructure other than DI
            // Determine a better technique
            services.AddInfrastructure();

            return services;
        }
    }
}