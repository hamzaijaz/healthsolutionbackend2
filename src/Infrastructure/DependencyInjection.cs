using System;
using System.Diagnostics.CodeAnalysis;
using MyHealthSolution.Service.Application.Common.Interfaces;
using MyHealthSolution.Service.Infrastructure.Context;
using MyHealthSolution.Service.Infrastructure.Persistence;
using MyHealthSolution.Service.Infrastructure.ServiceBus;
using MyHealthSolution.Service.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace MyHealthSolution.Service.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddTransient<IDateTime, DateTimeService>();

            var serviceProvider = services.BuildServiceProvider();
            IConfiguration configuration = serviceProvider.GetService<IConfiguration>();

            if (configuration.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("AdminDb"));
            }
            else
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(
                        configuration.GetValue<string>("ConnectionString")));//,
                                                                             //b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            }
            services.AddScoped<IApplicationDbContext>(provider => provider.GetService<ApplicationDbContext>());

            // note: the below dependencies use a scope context (per call scope)
            services.AddScoped<ICallContext, MutableCallContext>();
            services.AddScoped<IMessageEnricher, AzureServiceBusCausalityEnricher>();

            services.AddSingleton<JsonSerializer>();

            var recaptchaConfig = configuration.Get<RecaptchaConfig>();
            services.AddSingleton(recaptchaConfig);

            services.AddSingleton<IServiceBusConfiguration, ServiceBusConfiguration>();
            services.AddSingleton<IBusEndpointFactory, AzureServiceBusEndpointFactory>();

            services.AddHealthChecks();

            //ADD HttpClient
            services.AddHttpClient<ICaptchaVerifier, CaptchaVerifier>(x =>
            {
                x.BaseAddress = new Uri("https://www.google.com/recaptcha/api/siteverify");
                x.DefaultRequestHeaders.Add("accept", "application/json");
            });

            return services;
        }
    }
}
