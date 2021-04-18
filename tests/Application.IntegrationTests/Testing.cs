using System;
using System.IO;
using System.Threading.Tasks;
using MyHealthSolution.Service.FunctionApp;
using MyHealthSolution.Service.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Linq;
using System.Linq.Expressions;
using Respawn;
using MyHealthSolution.Service.Application.Common.Interfaces;
using Moq;
using System.Collections.Generic;
using MyHealthSolution.Service.Infrastructure.Context;
using Microsoft.Extensions.Logging;

namespace Application.IntegrationTests
{
    public class Testing : IDisposable
    {
        private static IConfiguration _configuration;
        private static IServiceScopeFactory _scopeFactory;
        private static Checkpoint _checkpoint;
        public static string TestUser = "RaiseTesting";

        public Testing() : base()
        {
            RunBeforeAnyTests();
        }

        public void RunBeforeAnyTests()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables();

            _configuration = builder.Build();

            var startup = new Startup();

            var services = new ServiceCollection();

            services.AddLogging();
            
            services.AddSingleton(_configuration);

            startup.ConfigureServices(services);

            ReplaceServiceRegistrations(services);

            _scopeFactory = services.BuildServiceProvider().GetService<IServiceScopeFactory>();

            _checkpoint = new Checkpoint
            {
                TablesToIgnore = new [] { "_SchemaVersions" }
            };

            EnsureDatabase();
        }

        private static void ReplaceServiceRegistrations(IServiceCollection services)
        {
            // use this method to replace any registrations with mocks.
            // usually this would be any external APIs or Message Bus interactions

            // Replace service registration for IBusEndpointFactory
            // Remove existing registration
            var busEndpointFactory = services.FirstOrDefault(d =>
                d.ServiceType == typeof(IBusEndpointFactory));


            services.Remove(busEndpointFactory);
            services.AddSingleton<IBusEndpointFactory, InMemoryServiceBusEndpointFactory>();

            // setup call context
            var existingCallContext = services.FirstOrDefault(d =>
                d.ServiceType == typeof(ICallContext));

            services.Remove(existingCallContext);
            var callContext = new MutableCallContext();
            callContext.OriginatingUsername = TestUser;
            callContext.AuthenticationType = string.Empty;
            callContext.CorrelationId = Guid.NewGuid();

            services.AddTransient<ICallContext, MutableCallContext>((sp) => callContext);

            // removing captcha service
            var recaptchaService = services.FirstOrDefault(d =>
                    d.ServiceType == typeof(ICaptchaVerifier));
            services.Remove(recaptchaService);

            // mocking captcha service
            var mockingCaptchaService = new Mock<ICaptchaVerifier>();
            mockingCaptchaService.Setup(_ => _.VerifyCaptcha(It.IsAny<string>())).ReturnsAsync(true);
            services.AddScoped(provider => mockingCaptchaService.Object);
        }

        private static void EnsureDatabase()
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

            if(context.Database.IsSqlServer())
                context.Database.EnsureCreated();
        }

        // NOTE: We are passing the XUnit TestHelper here due to logging the validation exceptions.
        public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, Xunit.Abstractions.ITestOutputHelper output)
        {
            using var scope = _scopeFactory.CreateScope();

            var mediator = scope.ServiceProvider.GetService<IMediator>();

            try
            {
                return await mediator.Send(request);    
            }
            catch (MyHealthSolution.Service.Application.Common.Exceptions.ValidationException ex)
            {
                // note: this helps debug validation errors which can occur in tests
                output.WriteLine($"ValidationErrors:{Newtonsoft.Json.JsonConvert.SerializeObject(ex.Errors, Newtonsoft.Json.Formatting.Indented)}");
                throw;
            }
        }

        public static async Task ResetState()
        {
            using var scope = _scopeFactory.CreateScope();

            await _checkpoint.Reset(_configuration.GetValue<string>("ConnectionString"));

            // Service Bus
            var serviceBus = scope.ServiceProvider.GetService<IBusEndpointFactory>() as InMemoryServiceBusEndpointFactory;
            serviceBus.ResetState();
        }
        public static async Task<TEntity> FindAsync<TEntity>(int id)
            where TEntity : class
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

            return await context.FindAsync<TEntity>(id);
        }

        public static async Task<TEntity> FirstOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> predicate = null)
            where TEntity : class
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

            if (predicate != null)
                return await context.Set<TEntity>().FirstOrDefaultAsync<TEntity>(predicate);

            return await context.Set<TEntity>().FirstOrDefaultAsync<TEntity>();
        }

        public static async Task<TEntity> FirstOrDefaultAsync<TEntity, TProperty>(Expression<Func<TEntity, bool>> predicate, 
                                                                Expression<Func<TEntity, TProperty>> navigationPropertyPath)
            where TEntity : class
            where TProperty : class
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

            return await context.Set<TEntity>().Include(navigationPropertyPath).FirstOrDefaultAsync<TEntity>(predicate);
        }

        public static async Task<List<TEntity>> GetAsync<TEntity>(Expression<Func<TEntity, bool>> predicate)
            where TEntity : class
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

            return await context.Set<TEntity>().Where<TEntity>(predicate).ToListAsync();
        }

        public static async Task<List<TEntity>> GetAsync<TEntity, TProperty>(Expression<Func<TEntity, bool>> predicate,
                                                                Expression<Func<TEntity, TProperty>> navigationPropertyPath)
            where TEntity : class
            where TProperty : class
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

            return await context.Set<TEntity>().Include(navigationPropertyPath).Where<TEntity>(predicate).ToListAsync();
        }

        public static async Task<List<TEntity>> GetAsync<TEntity, TProperty, TProperty2>(Expression<Func<TEntity, bool>> predicate,
                                                                Expression<Func<TEntity, TProperty>> navigationPropertyPath,
                                                                Expression<Func<TEntity, TProperty2>> navigationPropertyPath2)
            where TEntity : class
            where TProperty : class
            where TProperty2 : class
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

            return await context.Set<TEntity>().Include(navigationPropertyPath).Include(navigationPropertyPath2).Where<TEntity>(predicate).ToListAsync();
        }

        public static async Task AddAsync<TEntity>(TEntity entity)
            where TEntity : class
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

            context.Add(entity);

            await context.SaveChangesAsync();
        }

        public static async Task UpdateAsync<TEntity>(TEntity entity)
            where TEntity : class
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

            context.Update(entity);

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Gets a list of service bus messages based on T
        /// </summary>
        /// <typeparam name="T">Service Bus Message Type</typeparam>
        public static Task<IEnumerable<T>> GetServiceBusMessagesAsync<T>(string endpointName, Func<T, bool> predicate = null) where T : class, new()
        {
            using var scope = _scopeFactory.CreateScope();

            var serviceBus = scope.ServiceProvider.GetService<IBusEndpointFactory>() as InMemoryServiceBusEndpointFactory;
            var logger = scope.ServiceProvider.GetService<ILogger<Testing>>();

            if(serviceBus == null)
            {
                throw new ArgumentException($"Make sure the ReplaceServiceRegistrations includes the registration of IBusEndpointFactory.");
            }
            var endpoint = serviceBus.Endpoints.FirstOrDefault(s => s.Key.Equals(endpointName));
            if (endpoint.Value == null)
            {
                logger.LogWarning($"No messages were captured for ServiceBus endpoint named '{endpointName}'. Make sure the endpoint name supplied in the test is valid.");
                return Task.FromResult(Enumerable.Empty<T>());
            }
            var typedMessages = endpoint.Value.Messages.Where(m => m.Key == typeof(T));

            var actualTypedMessages = (typedMessages.Select(_ => _.Value).Cast<T>());
            if (predicate != null)
            {
                actualTypedMessages = actualTypedMessages.Where(predicate);
            }

            return Task.FromResult(actualTypedMessages);
        }

        /// <summary>
        /// Find a specific service bus message based on T.
        /// </summary>
        /// <typeparam name="T">Service bus message type</typeparam>
        public static Task<T> FindServiceBusMessageAsync<T>(string endpointName, Func<T, bool> predicate = null) where T: class, new()
        {
            using var scope = _scopeFactory.CreateScope();

            var serviceBus = scope.ServiceProvider.GetService<IBusEndpointFactory>() as InMemoryServiceBusEndpointFactory;
            var logger = scope.ServiceProvider.GetService<ILogger<Testing>>();

            if(serviceBus == null)
            {
                throw new ArgumentException($"Make sure the ReplaceServiceRegistrations includes the registration of IBusEndpointFactory.");
            }

            var endpoint = serviceBus.Endpoints.FirstOrDefault(s => s.Key.Equals(endpointName));
            if(endpoint.Value == null)
            {
                logger.LogWarning($"No messages were captured for ServiceBus endpoint named '{endpointName}'. Make sure the endpoint name supplied in the test is valid.");
                return Task.FromResult<T>(null);
            }
            var typedMessage = endpoint.Value.Messages.FirstOrDefault(m => m.Key == typeof(T));
            if(typedMessage.Value == null)
            {
                logger.LogWarning($"No messages were captured for ServiceBus endpoint named '{endpointName}' and type '{typeof(T).FullName}'. Make sure the Generic Type argument supplied in the test is valid.");
                return Task.FromResult<T>(null);
            }

            var typedMessages =(typedMessage.Value.Cast<T>());
            if(predicate != null)
            {
                typedMessages = typedMessages.Where(predicate);
            }
            
            if(typedMessages.Count() > 1)
            {
                throw new ArgumentException($"{typedMessages.Count()} messages were captured for ServiceBus endpoint named '{endpointName}' and type '{typeof(T).FullName}'. Provide a predicate which ensures the result returns a unique item.");
            }
            return Task.FromResult(typedMessages.FirstOrDefault());
        }

        public static Task SetCallContext<TProperty, TValue>(Expression<Func<ICallContext, TProperty>> property, TValue value)
        //          where TProperty : class
        //where TValue : struct
        {
            using var scope = _scopeFactory.CreateScope();

            var callContext = scope.ServiceProvider.GetService<ICallContext>();

            var memberExpression = (MemberExpression)property.Body;
            var propertyInfo = (System.Reflection.PropertyInfo)memberExpression.Member;

            propertyInfo.SetValue(callContext, value);

            return Task.CompletedTask;
        }

        public Task RunAfterAnyTests()
        {
            return ResetState();
        }

        public void Dispose()
        {
            RunAfterAnyTests().Wait();
        }
    }

    [CollectionDefinition("Tests")]
    public class TestCollection : ICollectionFixture<Testing> {}
}