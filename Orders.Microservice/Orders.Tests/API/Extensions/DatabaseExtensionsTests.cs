using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orders.API.Extensions;
using Orders.Infrastructure.Data;

namespace Orders.Tests.API.Extensions
{
    public class DatabaseExtensionsTests
    {
        [Fact]
        public void ApplyMigrations_WithoutLogger_ThrowsException()
        {
            var services = new ServiceCollection();
            services.AddDbContext<OrdersDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

            var serviceProvider = services.BuildServiceProvider();
            var appBuilder = new ApplicationBuilder(serviceProvider);

            Action act = () => appBuilder.ApplyMigrations();

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*ILogger*");
        }

        [Fact]
        public void ApplyMigrations_WithoutDbContext_ThrowsException()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());

            var serviceProvider = services.BuildServiceProvider();
            var appBuilder = new ApplicationBuilder(serviceProvider);

            Action act = () => appBuilder.ApplyMigrations();

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*OrdersDbContext*");
        }

        [Fact]
        public void ApplyMigrations_WithEmptyServiceProvider_ThrowsException()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var appBuilder = new ApplicationBuilder(serviceProvider);

            Action act = () => appBuilder.ApplyMigrations();

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ApplyMigrations_WithInMemoryDatabase_ThrowsInvalidOperationException()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddDbContext<OrdersDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

            var serviceProvider = services.BuildServiceProvider();
            var appBuilder = new ApplicationBuilder(serviceProvider);

            Action act = () => appBuilder.ApplyMigrations();

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Relational*");
        }

        [Fact]
        public void ApplyMigrations_IsExtensionMethod()
        {
            var methodInfo = typeof(DatabaseExtensions).GetMethod("ApplyMigrations");

            methodInfo.Should().NotBeNull();
            methodInfo!.IsStatic.Should().BeTrue();
            methodInfo.GetParameters().Should().HaveCount(1);
            methodInfo.GetParameters()[0].ParameterType.Should().Be(typeof(IApplicationBuilder));
            methodInfo.ReturnType.Should().Be(typeof(IApplicationBuilder));
        }

        [Fact]
        public void ApplyMigrations_HasCorrectMethodSignature()
        {
            var methodInfo = typeof(DatabaseExtensions).GetMethod("ApplyMigrations");

            methodInfo.Should().NotBeNull();
            methodInfo!.Name.Should().Be("ApplyMigrations");

            var parameters = methodInfo.GetParameters();
            parameters.Should().HaveCount(1);
            parameters[0].ParameterType.Should().Be(typeof(IApplicationBuilder));
        }

        [Fact]
        public void DatabaseExtensions_IsStaticClass()
        {
            var type = typeof(DatabaseExtensions);

            type.IsAbstract.Should().BeTrue();
            type.IsSealed.Should().BeTrue();
            type.IsClass.Should().BeTrue();
        }

        [Fact]
        public void ApplyMigrations_ReturnsIApplicationBuilder()
        {
            var methodInfo = typeof(DatabaseExtensions).GetMethod("ApplyMigrations");

            methodInfo.Should().NotBeNull();
            methodInfo!.ReturnType.Should().Be(typeof(IApplicationBuilder));
        }

        [Fact]
        public void ApplyMigrations_AcceptsIApplicationBuilderParameter()
        {
            var methodInfo = typeof(DatabaseExtensions).GetMethod("ApplyMigrations");

            methodInfo.Should().NotBeNull();
            var parameters = methodInfo!.GetParameters();
            parameters.Should().HaveCount(1);
            parameters[0].ParameterType.Should().Be(typeof(IApplicationBuilder));
            parameters[0].Name.Should().Be("app");
        }

        [Fact]
        public void DatabaseExtensions_ContainsApplyMigrationsMethod()
        {
            var methods = typeof(DatabaseExtensions).GetMethods();
            var applyMigrationsMethod = methods.FirstOrDefault(m => m.Name == "ApplyMigrations");

            applyMigrationsMethod.Should().NotBeNull();
        }

        [Fact]
        public void DatabaseExtensions_ApplyMigrations_IsPublicMethod()
        {
            var methodInfo = typeof(DatabaseExtensions).GetMethod("ApplyMigrations");

            methodInfo.Should().NotBeNull();
            methodInfo!.IsPublic.Should().BeTrue();
        }

        [Fact]
        public void ApplyMigrations_WithNullApplicationBuilder_ThrowsArgumentNullException()
        {
            IApplicationBuilder? nullBuilder = null;

            Action act = () => nullBuilder!.ApplyMigrations();

            act.Should().Throw<NullReferenceException>();
        }

        [Fact]
        public void DatabaseExtensions_HasOnlyOnePublicMethod()
        {
            var publicMethods = typeof(DatabaseExtensions)
                .GetMethods()
                .Where(m => m.IsPublic && m.DeclaringType == typeof(DatabaseExtensions));

            publicMethods.Should().HaveCount(1);
            publicMethods.First().Name.Should().Be("ApplyMigrations");
        }

        [Fact]
        public void ApplyMigrations_MethodExists_InCorrectNamespace()
        {
            var type = typeof(DatabaseExtensions);

            type.Namespace.Should().Be("Orders.API.Extensions");
        }

        [Fact]
        public void DatabaseExtensions_IsInCorrectAssembly()
        {
            var type = typeof(DatabaseExtensions);

            type.Assembly.GetName().Name.Should().Be("Orders.API");
        }

        [Fact]
        public void ApplyMigrations_WithMissingServices_ThrowsMeaningfulException()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var appBuilder = new ApplicationBuilder(serviceProvider);

            Action act = () => appBuilder.ApplyMigrations();

            act.Should().Throw<InvalidOperationException>()
                .And.Message.Should().Contain("service");
        }

        [Fact]
        public void DatabaseExtensions_ClassAttributes_AreCorrect()
        {
            var type = typeof(DatabaseExtensions);

            type.IsPublic.Should().BeTrue();
            type.IsAbstract.Should().BeTrue();
            type.IsSealed.Should().BeTrue();
        }

        [Fact]
        public void ApplyMigrations_ThrowsWhen_ServicesNotConfigured()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var appBuilder = new ApplicationBuilder(serviceProvider);

            Action act = () => appBuilder.ApplyMigrations();

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ApplyMigrations_ThrowsWhen_OnlyLoggerConfigured()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            var serviceProvider = services.BuildServiceProvider();
            var appBuilder = new ApplicationBuilder(serviceProvider);

            Action act = () => appBuilder.ApplyMigrations();

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ApplyMigrations_ThrowsWhen_OnlyDbContextConfigured()
        {
            var services = new ServiceCollection();
            services.AddDbContext<OrdersDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

            var serviceProvider = services.BuildServiceProvider();
            var appBuilder = new ApplicationBuilder(serviceProvider);

            Action act = () => appBuilder.ApplyMigrations();

            act.Should().Throw<InvalidOperationException>();
        }
    }
}