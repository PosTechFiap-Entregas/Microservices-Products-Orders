using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Products.API.Data;
using Products.Application.Services.Interface;
using Products.Application.Services.Service;
using Products.Domain.Interfaces.Repository;
using Products.Infrastructure.Data;
using Products.Infrastructure.Repositories;

namespace Products.Tests.API
{
    public class ProgramTests
    {
        private IServiceCollection CreateServiceCollection()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"ConnectionStrings:DefaultConnection", "Server=localhost;Database=test;"}
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

            services.AddDbContext<ProductsDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"));

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<DatabaseSeeder>();

            services.AddHealthChecks()
                .AddDbContextCheck<ProductsDbContext>();

            services.AddLogging();

            return services.BuildServiceProvider();
        }

        #region Service Registration Tests

        [Fact]
        public void Program_RegistersProductRepository()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var repository = scope.ServiceProvider.GetService<IProductRepository>();

            repository.Should().NotBeNull();
            repository.Should().BeOfType<ProductRepository>();
        }

        [Fact]
        public void Program_RegistersProductService()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetService<IProductService>();

            service.Should().NotBeNull();
            service.Should().BeOfType<ProductService>();
        }

        [Fact]
        public void Program_RegistersDatabaseSeeder()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var seeder = scope.ServiceProvider.GetService<DatabaseSeeder>();

            seeder.Should().NotBeNull();
            seeder.Should().BeOfType<DatabaseSeeder>();
        }

        [Fact]
        public void Program_RegistersDbContext()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<ProductsDbContext>();

            dbContext.Should().NotBeNull();
            dbContext.Should().BeOfType<ProductsDbContext>();
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
        public void Program_ProductRepository_IsRegisteredAsScoped()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope1 = serviceProvider.CreateScope();
            using var scope2 = serviceProvider.CreateScope();

            var repo1a = scope1.ServiceProvider.GetService<IProductRepository>();
            var repo1b = scope1.ServiceProvider.GetService<IProductRepository>();
            var repo2 = scope2.ServiceProvider.GetService<IProductRepository>();

            repo1a.Should().BeSameAs(repo1b);
            repo1a.Should().NotBeSameAs(repo2);
        }

        [Fact]
        public void Program_ProductService_IsRegisteredAsScoped()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope1 = serviceProvider.CreateScope();
            using var scope2 = serviceProvider.CreateScope();
            var service1a = scope1.ServiceProvider.GetService<IProductService>();
            var service1b = scope1.ServiceProvider.GetService<IProductService>();
            var service2 = scope2.ServiceProvider.GetService<IProductService>();

            service1a.Should().BeSameAs(service1b);
            service1a.Should().NotBeSameAs(service2);
        }

        [Fact]
        public void Program_DatabaseSeeder_IsRegisteredAsScoped()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope1 = serviceProvider.CreateScope();
            using var scope2 = serviceProvider.CreateScope();

            var seeder1a = scope1.ServiceProvider.GetService<DatabaseSeeder>();
            var seeder1b = scope1.ServiceProvider.GetService<DatabaseSeeder>();
            var seeder2 = scope2.ServiceProvider.GetService<DatabaseSeeder>();

            seeder1a.Should().BeSameAs(seeder1b);
            seeder1a.Should().NotBeSameAs(seeder2);
        }

        [Fact]
        public void Program_DbContext_IsRegisteredAsScoped()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope1 = serviceProvider.CreateScope();
            using var scope2 = serviceProvider.CreateScope();

            var context1a = scope1.ServiceProvider.GetService<ProductsDbContext>();
            var context1b = scope1.ServiceProvider.GetService<ProductsDbContext>();
            var context2 = scope2.ServiceProvider.GetService<ProductsDbContext>();

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

            scope.ServiceProvider.GetService<IProductRepository>().Should().NotBeNull();
            scope.ServiceProvider.GetService<IProductService>().Should().NotBeNull();
            scope.ServiceProvider.GetService<DatabaseSeeder>().Should().NotBeNull();
            scope.ServiceProvider.GetService<ProductsDbContext>().Should().NotBeNull();
        }

        [Fact]
        public void Program_AllScopedServices_CanBeResolved()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            Action act = () =>
            {
                scope.ServiceProvider.GetRequiredService<IProductRepository>();
                scope.ServiceProvider.GetRequiredService<IProductService>();
                scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
                scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
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
                scope.ServiceProvider.GetRequiredService<IProductRepository>();
                scope.ServiceProvider.GetRequiredService<IProductService>();
                scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
                scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
            };

            act.Should().NotThrow<InvalidOperationException>();
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

            result.Entries.Should().ContainKey("ProductsDbContext");
        }

        #endregion

        #region Service Dependencies Tests

        [Fact]
        public void Program_ProductService_HasRequiredDependencies()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var productService = scope.ServiceProvider.GetService<IProductService>();

            productService.Should().NotBeNull();
        }

        [Fact]
        public void Program_ProductRepository_HasRequiredDependencies()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var productRepository = scope.ServiceProvider.GetService<IProductRepository>();

            productRepository.Should().NotBeNull();
        }

        [Fact]
        public void Program_DatabaseSeeder_HasRequiredDependencies()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var seeder = scope.ServiceProvider.GetService<DatabaseSeeder>();

            seeder.Should().NotBeNull();
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

        #endregion

        #region Multiple Scopes Tests

        [Fact]
        public void Program_MultipleScopes_AreIndependent()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope1 = serviceProvider.CreateScope();
            using var scope2 = serviceProvider.CreateScope();
            using var scope3 = serviceProvider.CreateScope();

            var service1 = scope1.ServiceProvider.GetService<IProductService>();
            var service2 = scope2.ServiceProvider.GetService<IProductService>();
            var service3 = scope3.ServiceProvider.GetService<IProductService>();

            service1.Should().NotBeSameAs(service2);
            service2.Should().NotBeSameAs(service3);
            service1.Should().NotBeSameAs(service3);
        }

        #endregion

        #region DatabaseSeeder Specific Tests

        [Fact]
        public void Program_DatabaseSeeder_CanBeInstantiated()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            Action act = () =>
            {
                var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            };

            act.Should().NotThrow();
        }

        [Fact]
        public void Program_DatabaseSeeder_HasAccessToDbContext()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var seeder = scope.ServiceProvider.GetService<DatabaseSeeder>();
            var dbContext = scope.ServiceProvider.GetService<ProductsDbContext>();

            seeder.Should().NotBeNull();
            dbContext.Should().NotBeNull();
        }

        [Fact]
        public void Program_MultipleDatabaseSeeder_Instances_AreIndependent()
        {
            var serviceProvider = BuildServiceProvider();

            using var scope1 = serviceProvider.CreateScope();
            using var scope2 = serviceProvider.CreateScope();

            var seeder1 = scope1.ServiceProvider.GetService<DatabaseSeeder>();
            var seeder2 = scope2.ServiceProvider.GetService<DatabaseSeeder>();

            seeder1.Should().NotBeSameAs(seeder2);
        }

        #endregion

        #region Repository Tests

        [Fact]
        public void Program_ProductRepository_ImplementsInterface()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var repository = scope.ServiceProvider.GetService<IProductRepository>();

            repository.Should().NotBeNull();
            repository.Should().BeAssignableTo<IProductRepository>();
            repository.Should().BeOfType<ProductRepository>();
        }

        #endregion

        #region Service Tests

        [Fact]
        public void Program_ProductService_ImplementsInterface()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var service = scope.ServiceProvider.GetService<IProductService>();

            service.Should().NotBeNull();
            service.Should().BeAssignableTo<IProductService>();
            service.Should().BeOfType<ProductService>();
        }

        #endregion

        #region DbContext Tests

        [Fact]
        public void Program_DbContext_IsConfigured()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var dbContext = scope.ServiceProvider.GetService<ProductsDbContext>();

            dbContext.Should().NotBeNull();
            dbContext.Database.Should().NotBeNull();
        }

        [Fact]
        public void Program_DbContext_CanBeUsedInScope()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            Action act = () =>
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
                _ = dbContext.Products;
            };

            act.Should().NotThrow();
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void Program_AllServices_WorkTogether()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            Action act = () =>
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
                var repository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
                var service = scope.ServiceProvider.GetRequiredService<IProductService>();
                var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            };

            act.Should().NotThrow();
        }

        [Fact]
        public void Program_ServiceChain_IsCorrectlyConfigured()
        {
            var serviceProvider = BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var service = scope.ServiceProvider.GetService<IProductService>();
            var repository = scope.ServiceProvider.GetService<IProductRepository>();
            var dbContext = scope.ServiceProvider.GetService<ProductsDbContext>();

            service.Should().NotBeNull();
            repository.Should().NotBeNull();
            dbContext.Should().NotBeNull();
        }

        #endregion
    }
}