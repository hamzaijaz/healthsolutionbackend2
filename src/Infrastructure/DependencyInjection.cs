using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using CapitalRaising.RightsIssues.Service.Application.Common.Interfaces;
using CapitalRaising.RightsIssues.Service.Infrastructure.Context;
using CapitalRaising.RightsIssues.Service.Infrastructure.Files;
using CapitalRaising.RightsIssues.Service.Infrastructure.Persistence;
using CapitalRaising.RightsIssues.Service.Infrastructure.Persistence.Configurations;
using CapitalRaising.RightsIssues.Service.Infrastructure.ServiceBus;
using CapitalRaising.RightsIssues.Service.Infrastructure.Services;
using CapitalRaising.RightsIssues.Service.Infrastructure.Telemetry;
using HealthChecks.SqlServer;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;

namespace CapitalRaising.RightsIssues.Service.Infrastructure
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

            services.AddCsvFile(Assembly.GetExecutingAssembly());

            // note: the below dependencies use a scope context (per call scope)
            services.AddScoped<ICallContext, MutableCallContext>();
            services.AddScoped<IMessageEnricher, AzureServiceBusCausalityEnricher>();

            services.Configure<BlobStoreConfig>(configuration);
            services.AddTransient<IBlobDataStore, AzureBlobStorageDataStore>();

            services.AddSingleton<JsonSerializer>();

            services.AddSingleton<IZipFileBuilder, ZipFileBuilder>();

            services.AddSingleton<ICurrencyService, CurrencyService>();

            var recaptchaConfig = configuration.Get<RecaptchaConfig>();
            services.AddSingleton(recaptchaConfig);

            services.AddSingleton<IServiceBusConfiguration, ServiceBusConfiguration>();
            services.AddSingleton<IBusEndpointFactory, AzureServiceBusEndpointFactory>();

            var encryptionConfig = configuration.Get<EncryptionConfig>();
            services.AddSingleton(encryptionConfig);
            services.AddTransient<IEncryptionService, EncryptionService>();


            services.AddAppInsights(configuration);
            services.AddHealthChecks();

            //ADD HttpClient
            services.AddHttpClient<ICaptchaVerifier, CaptchaVerifier>(x =>
            {
                x.BaseAddress = new Uri("https://www.google.com/recaptcha/api/siteverify");
                x.DefaultRequestHeaders.Add("accept", "application/json");
            });

            return services;
        }

        private static IServiceCollection AddCsvFile(this IServiceCollection services, Assembly assembly)
        {
            // Register all class maps in the assembly
            var csvClassMap = typeof(CsvHelper.Configuration.ClassMap);

            var classMapTypes = assembly
                .GetExportedTypes()
                .Where(t => t.IsSubclassOf(csvClassMap))
                .ToList();

            foreach (var classMap in classMapTypes)
            {
                services.AddTransient(csvClassMap, classMap);
            }

            services.AddTransient<ICsvFileBuilder, CsvFileBuilder>();
            services.AddTransient<ICsvFileReader, CsvFileReader>();

            return services;
        }

        private static IServiceCollection AddAppInsights(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<TelemetryClient>((serviceProvider) =>
            {
                const string endpointAddress = "https://dc.services.visualstudio.com/v2/track";
                var instrumentationKey = configuration.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY");
                var telemetryConfiguration = new TelemetryConfiguration(
                    instrumentationKey,
                    new InMemoryChannel { EndpointAddress = endpointAddress });
                return new TelemetryClient(telemetryConfiguration);
            });
            services.AddSingleton<ITelemetryService, TelemetryService>();

            return services;
        }

        private static IServiceCollection AddHealthChecks(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var config = serviceProvider.GetService<IConfiguration>();

            var sqlServerHealthCheck = new SqlServerHealthCheck(config["ConnectionString"], "SELECT 1;");
            services.AddSingleton<IHealthCheck>(sqlServerHealthCheck);

            return services;
        }
    }
}
