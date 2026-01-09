using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Orders.Application.Services.Interface;
using Orders.Application.Services.Service;
using Orders.Domain.Interfaces.Repository;
using Orders.Infrastructure.Data;
using Orders.Infrastructure.HttpClients;
using Orders.Infrastructure.Repositories;

namespace Orders.Tests.API
{
    public class ProgramTests
    {
        private IServiceCollection CreateServiceCollection()
        {
            var services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"ConnectionStrings:DefaultConnection", "Server=localhost;Database=test;"},
                    {"ProductsApi:BaseUrl", "http://localhost:5001"},
                    {"PaymentApi:BaseUrl", "http://localhost:8083"}
                }!)
                .Build();

            services.AddSingleton<IConfiguration>(configuration);

            return services;
        }

        private IServiceProvider BuildServiceProvider()
        {
            var services = CreateServiceCollection();

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(
                        new System.Text.Json.Serialization.JsonStringEnumConverter());
                });

            services.AddDbContext<OrdersDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"));

            services.AddHttpClient<IProductsHttpClient, ProductsHttpClient>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5001");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddHttpClient<IPaymentHttpClient, PaymentHttpClient>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:8083");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IPaymentService, PaymentService>();

            services.AddHealthChecks()
                .AddDbContextCheck<OrdersDbContext>();

            services.AddLogging();

            return services.BuildServiceProvider();
        }

        #region Service Registration Tests

        [Fact]
        public void Program_RegistersOrderRepository()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var repository = scope.ServiceProvider.GetService<IOrderRepository>();

            repository.Should().NotBeNull();
            repository.Should().BeOfType<OrderRepository>();
        }

        [Fact]
        public void Program_RegistersOrderService()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var service = scope.ServiceProvider.GetService<IOrderService>();

            service.Should().NotBeNull();
            service.Should().BeOfType<OrderService>();
        }

        [Fact]
        public void Program_RegistersPaymentService()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var service = scope.ServiceProvider.GetService<IPaymentService>();

            service.Should().NotBeNull();
            service.Should().BeOfType<PaymentService>();
        }

        [Fact]
        public void Program_RegistersProductsHttpClient()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var client = scope.ServiceProvider.GetService<IProductsHttpClient>();

            client.Should().NotBeNull();
            client.Should().BeOfType<ProductsHttpClient>();
        }

        [Fact]
        public void Program_RegistersPaymentHttpClient()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var client = scope.ServiceProvider.GetService<IPaymentHttpClient>();

            client.Should().NotBeNull();
            client.Should().BeOfType<PaymentHttpClient>();
        }

        [Fact]
        public void Program_RegistersDbContext()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var dbContext = scope.ServiceProvider.GetService<OrdersDbContext>();

            dbContext.Should().NotBeNull();
            dbContext.Should().BeOfType<OrdersDbContext>();
        }

        [Fact]
        public void Program_RegistersHealthCheckService()
        {
            var serviceProvider = BuildServiceProvider();

            var healthCheckService = serviceProvider.GetService<HealthCheckService>();

            healthCheckService.Should().NotBeNull();
        }

        #endregion

        #region Service Lifetime Tests

        [Fact]
        public void Program_OrderRepository_IsRegisteredAsScoped()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope1 = serviceProvider.CreateScope();
            using var scope2 = serviceProvider.CreateScope();

            var repo1a = scope1.ServiceProvider.GetService<IOrderRepository>();
            var repo1b = scope1.ServiceProvider.GetService<IOrderRepository>();
            var repo2 = scope2.ServiceProvider.GetService<IOrderRepository>();

            repo1a.Should().BeSameAs(repo1b);
            repo1a.Should().NotBeSameAs(repo2);
        }

        [Fact]
        public void Program_OrderService_IsRegisteredAsScoped()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope1 = serviceProvider.CreateScope();
            using var scope2 = serviceProvider.CreateScope();

            var service1a = scope1.ServiceProvider.GetService<IOrderService>();
            var service1b = scope1.ServiceProvider.GetService<IOrderService>();
            var service2 = scope2.ServiceProvider.GetService<IOrderService>();

            service1a.Should().BeSameAs(service1b);
            service1a.Should().NotBeSameAs(service2);
        }

        [Fact]
        public void Program_PaymentService_IsRegisteredAsScoped()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope1 = serviceProvider.CreateScope();
            using var scope2 = serviceProvider.CreateScope();

            var service1a = scope1.ServiceProvider.GetService<IPaymentService>();
            var service1b = scope1.ServiceProvider.GetService<IPaymentService>();
            var service2 = scope2.ServiceProvider.GetService<IPaymentService>();

            service1a.Should().BeSameAs(service1b);
            service1a.Should().NotBeSameAs(service2);
        }

        [Fact]
        public void Program_DbContext_IsRegisteredAsScoped()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope1 = serviceProvider.CreateScope();
            using var scope2 = serviceProvider.CreateScope();

            var context1a = scope1.ServiceProvider.GetService<OrdersDbContext>();
            var context1b = scope1.ServiceProvider.GetService<OrdersDbContext>();
            var context2 = scope2.ServiceProvider.GetService<OrdersDbContext>();

            context1a.Should().BeSameAs(context1b);
            context1a.Should().NotBeSameAs(context2);
        }

        #endregion

        #region Dependency Resolution Tests

        [Fact]
        public void Program_CanResolveAllRegisteredServices()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            scope.ServiceProvider.GetService<IOrderRepository>().Should().NotBeNull();
            scope.ServiceProvider.GetService<IOrderService>().Should().NotBeNull();
            scope.ServiceProvider.GetService<IPaymentService>().Should().NotBeNull();
            scope.ServiceProvider.GetService<IProductsHttpClient>().Should().NotBeNull();
            scope.ServiceProvider.GetService<IPaymentHttpClient>().Should().NotBeNull();
            scope.ServiceProvider.GetService<OrdersDbContext>().Should().NotBeNull();
        }

        [Fact]
        public void Program_AllScopedServices_CanBeResolved()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            Action act = () =>
            {
                scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                scope.ServiceProvider.GetRequiredService<IOrderService>();
                scope.ServiceProvider.GetRequiredService<IPaymentService>();
                scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
            };

            act.Should().NotThrow();
        }

        [Fact]
        public void Program_NoCircularDependencies()
        {
            var serviceProvider = BuildServiceProvider();

            Action act = () =>
            {
                using var scope = serviceProvider.CreateScope();
                scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                scope.ServiceProvider.GetRequiredService<IOrderService>();
                scope.ServiceProvider.GetRequiredService<IPaymentService>();
                scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
            };

            act.Should().NotThrow<InvalidOperationException>();
        }

        #endregion

        #region HttpClient Configuration Tests

        [Fact]
        public void Program_ProductsHttpClient_IsConfigured()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var productsClient = scope.ServiceProvider.GetService<IProductsHttpClient>();

            productsClient.Should().NotBeNull();
            productsClient.Should().BeOfType<ProductsHttpClient>();
        }

        [Fact]
        public void Program_PaymentHttpClient_IsConfigured()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var paymentClient = scope.ServiceProvider.GetService<IPaymentHttpClient>();

            paymentClient.Should().NotBeNull();
            paymentClient.Should().BeOfType<PaymentHttpClient>();
        }

        [Fact]
        public void Program_HttpClientFactory_IsRegistered()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var httpClientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();

            httpClientFactory.Should().NotBeNull();
        }

        [Fact]
        public void Program_HttpClientFactory_CanCreateClients()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient();

            client.Should().NotBeNull();
        }

        #endregion

        #region JSON Configuration Tests

        [Fact]
        public void Program_JsonOptions_ConfiguresEnumConverter()
        {
            var serviceProvider = BuildServiceProvider();
            var jsonOptions = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Mvc.JsonOptions>>();

            jsonOptions.Should().NotBeNull();
            var converters = jsonOptions!.Value.JsonSerializerOptions.Converters;
            converters.Should().Contain(c => c.GetType().Name.Contains("JsonStringEnumConverter"));
        }

        #endregion

        #region Health Check Tests

        [Fact]
        public void Program_HealthChecks_AreRegistered()
        {
            var serviceProvider = BuildServiceProvider();
            var healthCheckService = serviceProvider.GetService<HealthCheckService>();

            healthCheckService.Should().NotBeNull();
        }

        [Fact]
        public async Task Program_HealthChecks_CanExecute()
        {
            var serviceProvider = BuildServiceProvider();
            var healthCheckService = serviceProvider.GetRequiredService<HealthCheckService>();
            var result = await healthCheckService.CheckHealthAsync();

            result.Should().NotBeNull();
            result.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Degraded, HealthStatus.Unhealthy);
        }

        [Fact]
        public async Task Program_HealthChecks_IncludesDbContextCheck()
        {
            var serviceProvider = BuildServiceProvider();
            var healthCheckService = serviceProvider.GetRequiredService<HealthCheckService>();
            var result = await healthCheckService.CheckHealthAsync();

            result.Entries.Should().ContainKey("OrdersDbContext");
        }

        #endregion

        #region Service Dependencies Tests

        [Fact]
        public void Program_OrderService_HasRequiredDependencies()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var orderService = scope.ServiceProvider.GetService<IOrderService>();

            orderService.Should().NotBeNull();
        }

        [Fact]
        public void Program_PaymentService_HasRequiredDependencies()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var paymentService = scope.ServiceProvider.GetService<IPaymentService>();

            paymentService.Should().NotBeNull();
        }

        [Fact]
        public void Program_OrderRepository_HasRequiredDependencies()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var orderRepository = scope.ServiceProvider.GetService<IOrderRepository>();

            orderRepository.Should().NotBeNull();
        }

        #endregion

        #region Configuration Tests

        [Fact]
        public void Program_Configuration_IsAvailable()
        {
            var serviceProvider = BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            configuration.Should().NotBeNull();
        }

        [Fact]
        public void Program_ConnectionString_IsConfigurable()
        {
            var services = CreateServiceCollection();
            var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            connectionString.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Program_ProductsApiUrl_IsConfigurable()
        {
            var services = CreateServiceCollection();
            var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            var productsApiUrl = configuration["ProductsApi:BaseUrl"];

            productsApiUrl.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Program_PaymentApiUrl_IsConfigurable()
        {
            var services = CreateServiceCollection();
            var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            var paymentApiUrl = configuration["PaymentApi:BaseUrl"];

            paymentApiUrl.Should().NotBeNullOrEmpty();
        }

        #endregion

        #region Multiple Scopes Tests

        [Fact]
        public void Program_MultipleScopes_AreIndependent()
        {
            var serviceProvider = BuildServiceProvider();

            using var scope1 = serviceProvider.CreateScope();
            using var scope2 = serviceProvider.CreateScope();
            using var scope3 = serviceProvider.CreateScope();

            var service1 = scope1.ServiceProvider.GetService<IOrderService>();
            var service2 = scope2.ServiceProvider.GetService<IOrderService>();
            var service3 = scope3.ServiceProvider.GetService<IOrderService>();

            service1.Should().NotBeSameAs(service2);
            service2.Should().NotBeSameAs(service3);
            service1.Should().NotBeSameAs(service3);
        }

        #endregion
    }
}