using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;
using FluentAssertions;
using Moq;
using KeyForge.Infrastructure.DependencyInjection;
using KeyForge.Domain.Interfaces;
using KeyForge.Infrastructure.Data;
using KeyForge.Infrastructure.Repositories;
using KeyForge.Infrastructure.Services;
using KeyForge.Tests.Common;

namespace KeyForge.Tests.ComponentTests.Infrastructure.DependencyInjection
{
    /// <summary>
    /// 依赖注入组件测试
    /// 测试Infrastructure层的依赖注入配置和服务注册
    /// </summary>
    public class DependencyInjectionComponentTests : TestBase
    {
        private readonly IServiceCollection _services;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public DependencyInjectionComponentTests()
        {
            _services = new ServiceCollection();
            _mockConfiguration = new Mock<IConfiguration>();
        }

        #region AddInfrastructure测试

        [Fact]
        public void AddInfrastructure_ShouldRegisterAllRequiredServices()
        {
            // Arrange
            SetupConfiguration();

            // Act
            _services.AddInfrastructure(_mockConfiguration.Object);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            // 验证DbContext已注册
            serviceProvider.GetService<KeyForgeDbContext>().Should().NotBeNull();

            // 验证仓储已注册
            serviceProvider.GetService<IScriptRepository>().Should().NotBeNull();
            serviceProvider.GetService<IImageTemplateRepository>().Should().NotBeNull();
            serviceProvider.GetService<IStateMachineRepository>().Should().NotBeNull();
            serviceProvider.GetService<IGameActionRepository>().Should().NotBeNull();
            serviceProvider.GetService<IDecisionRuleRepository>().Should().NotBeNull();
            serviceProvider.GetService<IStateRepository>().Should().NotBeNull();

            // 验证领域服务已注册
            serviceProvider.GetService<IScriptPlayerService>().Should().NotBeNull();
            serviceProvider.GetService<IImageRecognitionService>().Should().NotBeNull();
            serviceProvider.GetService<IInputSimulationService>().Should().NotBeNull();
            serviceProvider.GetService<IStateMachineService>().Should().NotBeNull();
            serviceProvider.GetService<IDecisionEngineService>().Should().NotBeNull();

            // 验证基础设施服务已注册
            serviceProvider.GetService<IFileStorageService>().Should().NotBeNull();
            serviceProvider.GetService<ILoggingService>().Should().NotBeNull();
            serviceProvider.GetService<IConfigurationService>().Should().NotBeNull();
            serviceProvider.GetService<ICacheRepository>().Should().NotBeNull();
            serviceProvider.GetService<IFileRepository>().Should().NotBeNull();
            serviceProvider.GetService<IConfigurationRepository>().Should().NotBeNull();
            serviceProvider.GetService<ILogRepository>().Should().NotBeNull();

            // 验证外部服务已注册
            serviceProvider.GetService<IHotkeyManagerService>().Should().NotBeNull();
            serviceProvider.GetService<IPerformanceService>().Should().NotBeNull();
            serviceProvider.GetService<IErrorHandlerService>().Should().NotBeNull();
            serviceProvider.GetService<ISerializationService>().Should().NotBeNull();

            // 验证工作单元已注册
            serviceProvider.GetService<IUnitOfWork>().Should().NotBeNull();
        }

        [Fact]
        public void AddInfrastructure_ShouldUseCorrectConnectionStrings()
        {
            // Arrange
            var expectedConnectionString = "Data Source=test.db";
            _mockConfiguration.Setup(config => config.GetConnectionString("DefaultConnection"))
                .Returns(expectedConnectionString);

            // Act
            _services.AddInfrastructure(_mockConfiguration.Object);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var dbContext = serviceProvider.GetService<KeyForgeDbContext>();
            dbContext.Should().NotBeNull();
        }

        [Fact]
        public void AddInfrastructure_WithDevelopmentEnvironment_ShouldEnableSensitiveDataLogging()
        {
            // Arrange
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            SetupConfiguration();

            // Act
            _services.AddInfrastructure(_mockConfiguration.Object);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var dbContext = serviceProvider.GetService<KeyForgeDbContext>();
            dbContext.Should().NotBeNull();
            
            // 清理环境变量
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }

        [Fact]
        public void AddInfrastructure_WithProductionEnvironment_ShouldNotEnableSensitiveDataLogging()
        {
            // Arrange
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
            SetupConfiguration();

            // Act
            _services.AddInfrastructure(_mockConfiguration.Object);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var dbContext = serviceProvider.GetService<KeyForgeDbContext>();
            dbContext.Should().NotBeNull();
            
            // 清理环境变量
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }

        [Fact]
        public void AddInfrastructure_ShouldRegisterScopedServices()
        {
            // Arrange
            SetupConfiguration();

            // Act
            _services.AddInfrastructure(_mockConfiguration.Object);
            var serviceProvider = _services.BuildServiceProvider();

            // 验证Scoped服务生命周期
            var scope1 = serviceProvider.CreateScope();
            var scope2 = serviceProvider.CreateScope();

            var repo1 = scope1.ServiceProvider.GetRequiredService<IScriptRepository>();
            var repo2 = scope2.ServiceProvider.GetRequiredService<IScriptRepository>();

            repo1.Should().NotBeSameAs(repo2);
        }

        #endregion

        #region AddApplication测试

        [Fact]
        public void AddApplication_ShouldRegisterApplicationServices()
        {
            // Arrange
            SetupConfiguration();

            // Act
            _services.AddInfrastructure(_mockConfiguration.Object);
            _services.AddApplication();
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            // 验证应用服务已注册
            serviceProvider.GetService<IScriptApplicationService>().Should().NotBeNull();
            serviceProvider.GetService<IStateMachineApplicationService>().Should().NotBeNull();
            serviceProvider.GetService<IImageTemplateApplicationService>().Should().NotBeNull();

            // 验证MediatR已注册
            serviceProvider.GetServices<IMediator>().Should().NotBeEmpty();
        }

        #endregion

        #region AddLogging测试

        [Fact]
        public void AddLogging_ShouldConfigureLoggingServices()
        {
            // Arrange
            SetupConfiguration();

            // Act
            _services.AddLogging(_mockConfiguration.Object);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            loggerFactory.Should().NotBeNull();
        }

        #endregion

        #region AddEnvironmentServices测试

        [Theory]
        [InlineData("Development")]
        [InlineData("Production")]
        [InlineData("Test")]
        [InlineData("Staging")]
        public void AddEnvironmentServices_ShouldConfigureServicesBasedOnEnvironment(string environment)
        {
            // Arrange
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
            SetupConfiguration();

            // Act
            _services.AddEnvironmentServices(_mockConfiguration.Object);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var dbContext = serviceProvider.GetService<KeyForgeDbContext>();
            dbContext.Should().NotBeNull();
            
            // 清理环境变量
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }

        [Fact]
        public void AddEnvironmentServices_WithUnknownEnvironment_ShouldUseDevelopmentConfiguration()
        {
            // Arrange
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Unknown");
            SetupConfiguration();

            // Act
            _services.AddEnvironmentServices(_mockConfiguration.Object);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var dbContext = serviceProvider.GetService<KeyForgeDbContext>();
            dbContext.Should().NotBeNull();
            
            // 清理环境变量
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }

        #endregion

        #region AddDevelopmentServices测试

        [Fact]
        public void AddDevelopmentServices_ShouldUseDevelopmentDatabase()
        {
            // Act
            _services.AddDevelopmentServices();
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var dbContext = serviceProvider.GetService<KeyForgeDbContext>();
            dbContext.Should().NotBeNull();
        }

        #endregion

        #region AddProductionServices测试

        [Fact]
        public void AddProductionServices_ShouldRequireConnectionString()
        {
            // Arrange
            Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", null);

            // Act & Assert
            var action = () => _services.AddProductionServices();
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Database connection string not configured for production");
            
            // 清理环境变量
            Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", null);
        }

        [Fact]
        public void AddProductionServices_WithConnectionString_ShouldConfigureProductionDatabase()
        {
            // Arrange
            Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", "Data Source=production.db");

            // Act
            _services.AddProductionServices();
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var dbContext = serviceProvider.GetService<KeyForgeDbContext>();
            dbContext.Should().NotBeNull();
            
            // 清理环境变量
            Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", null);
        }

        #endregion

        #region AddTestServices测试

        [Fact]
        public void AddTestServices_ShouldUseInMemoryDatabase()
        {
            // Act
            _services.AddTestServices();
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var dbContext = serviceProvider.GetService<KeyForgeDbContext>();
            dbContext.Should().NotBeNull();
        }

        [Fact]
        public void AddTestServices_ShouldRegisterTestRepositories()
        {
            // Arrange
            _services.AddTestServices();
            var serviceProvider = _services.BuildServiceProvider();

            // Act
            var scriptRepo = serviceProvider.GetService<IScriptRepository>();
            var imageTemplateRepo = serviceProvider.GetService<IImageTemplateRepository>();
            var stateMachineRepo = serviceProvider.GetService<IStateMachineRepository>();

            // Assert
            scriptRepo.Should().NotBeNull();
            imageTemplateRepo.Should().NotBeNull();
            stateMachineRepo.Should().NotBeNull();
        }

        #endregion

        #region ConfigureApplicationServices测试

        [Fact]
        public void ConfigureApplicationServices_ShouldConfigureAllOptions()
        {
            // Arrange
            SetupConfiguration();

            // Act
            _services.ConfigureApplicationServices(_mockConfiguration.Object);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            serviceProvider.GetService<IOptions<DatabaseOptions>>().Should().NotBeNull();
            serviceProvider.GetService<IOptions<FileStorageOptions>>().Should().NotBeNull();
            serviceProvider.GetService<IOptions<LoggingOptions>>().Should().NotBeNull();
            serviceProvider.GetService<IOptions<CacheOptions>>().Should().NotBeNull();
            serviceProvider.GetService<IOptions<PerformanceOptions>>().Should().NotBeNull();
            serviceProvider.GetService<IOptions<SecurityOptions>>().Should().NotBeNull();
        }

        #endregion

        #region AddValidationServices测试

        [Fact]
        public void AddValidationServices_ShouldRegisterValidationService()
        {
            // Act
            _services.AddValidationServices();
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            serviceProvider.GetService<IValidationService>().Should().NotBeNull();
        }

        #endregion

        #region AddCachingServices测试

        [Fact]
        public void AddCachingServices_WithMemoryCacheEnabled_ShouldRegisterMemoryCache()
        {
            // Arrange
            SetupConfiguration();
            _mockConfiguration.Setup(config => config.GetSection("Cache")).Returns(new Mock<IConfigurationSection>().Object);
            _mockConfiguration.Setup(config => config.GetSection("Cache:EnableMemoryCache")).Returns(new Mock<IConfigurationSection>().Object);
            _mockConfiguration.Setup(config => config.GetSection("Cache:EnableMemoryCache").Value).Returns("true");

            // Act
            _services.AddCachingServices(_mockConfiguration.Object);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            serviceProvider.GetService<IMemoryCache>().Should().NotBeNull();
        }

        [Fact]
        public void AddCachingServices_WithDistributedCacheEnabled_ShouldRegisterDistributedCache()
        {
            // Arrange
            SetupConfiguration();
            _mockConfiguration.Setup(config => config.GetSection("Cache")).Returns(new Mock<IConfigurationSection>().Object);
            _mockConfiguration.Setup(config => config.GetSection("Cache:EnableDistributedCache")).Returns(new Mock<IConfigurationSection>().Object);
            _mockConfiguration.Setup(config => config.GetSection("Cache:EnableDistributedCache").Value).Returns("true");
            _mockConfiguration.Setup(config => config.GetSection("Cache:RedisConnectionString")).Returns(new Mock<IConfigurationSection>().Object);
            _mockConfiguration.Setup(config => config.GetSection("Cache:RedisConnectionString").Value).Returns("localhost:6379");

            // Act
            _services.AddCachingServices(_mockConfiguration.Object);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            serviceProvider.GetService<IDistributedCache>().Should().NotBeNull();
        }

        #endregion

        #region AddFileStorageServices测试

        [Fact]
        public void AddFileStorageServices_ShouldCreateBaseDirectory()
        {
            // Arrange
            SetupConfiguration();
            var testBasePath = "test_storage";
            _mockConfiguration.Setup(config => config.GetSection("FileStorage")).Returns(new Mock<IConfigurationSection>().Object);
            _mockConfiguration.Setup(config => config.GetSection("FileStorage:BasePath")).Returns(new Mock<IConfigurationSection>().Object);
            _mockConfiguration.Setup(config => config.GetSection("FileStorage:BasePath").Value).Returns(testBasePath);

            // Act
            _services.AddFileStorageServices(_mockConfiguration.Object);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            var fileStorageService = serviceProvider.GetService<IFileStorageService>();
            fileStorageService.Should().NotBeNull();
            
            // 清理测试目录
            if (System.IO.Directory.Exists(testBasePath))
            {
                System.IO.Directory.Delete(testBasePath, true);
            }
        }

        #endregion

        #region AddPerformanceServices测试

        [Fact]
        public void AddPerformanceServices_WithMonitoringEnabled_ShouldRegisterPerformanceServices()
        {
            // Arrange
            SetupConfiguration();
            _mockConfiguration.Setup(config => config.GetSection("Performance")).Returns(new Mock<IConfigurationSection>().Object);
            _mockConfiguration.Setup(config => config.GetSection("Performance:EnableMonitoring")).Returns(new Mock<IConfigurationSection>().Object);
            _mockConfiguration.Setup(config => config.GetSection("Performance:EnableMonitoring").Value).Returns("true");

            // Act
            _services.AddPerformanceServices(_mockConfiguration.Object);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            serviceProvider.GetService<IPerformanceMonitor>().Should().NotBeNull();
            serviceProvider.GetService<IMetricsCollector>().Should().NotBeNull();
        }

        #endregion

        #region AddSecurityServices测试

        [Fact]
        public void AddSecurityServices_WithEncryptionEnabled_ShouldRegisterEncryptionService()
        {
            // Arrange
            SetupConfiguration();
            _mockConfiguration.Setup(config => config.GetSection("Security")).Returns(new Mock<IConfigurationSection>().Object);
            _mockConfiguration.Setup(config => config.GetSection("Security:EnableEncryption")).Returns(new Mock<IConfigurationSection>().Object);
            _mockConfiguration.Setup(config => config.GetSection("Security:EnableEncryption").Value).Returns("true");

            // Act
            _services.AddSecurityServices(_mockConfiguration.Object);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            serviceProvider.GetService<IEncryptionService>().Should().NotBeNull();
        }

        [Fact]
        public void AddSecurityServices_WithValidationEnabled_ShouldRegisterValidationService()
        {
            // Arrange
            SetupConfiguration();
            _mockConfiguration.Setup(config => config.GetSection("Security")).Returns(new Mock<IConfigurationSection>().Object);
            _mockConfiguration.Setup(config => config.GetSection("Security:EnableValidation")).Returns(new Mock<IConfigurationSection>().Object);
            _mockConfiguration.Setup(config => config.GetSection("Security:EnableValidation").Value).Returns("true");

            // Act
            _services.AddSecurityServices(_mockConfiguration.Object);
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            serviceProvider.GetService<IInputValidationService>().Should().NotBeNull();
        }

        #endregion

        #region InitializeDatabaseAsync测试

        [Fact]
        public async Task InitializeDatabaseAsync_ShouldMigrateDatabase()
        {
            // Arrange
            SetupConfiguration();
            _services.AddInfrastructure(_mockConfiguration.Object);
            var serviceProvider = _services.BuildServiceProvider();

            var mockDbContext = new Mock<KeyForgeDbContext>();
            mockDbContext.Setup(db => db.Database.MigrateAsync()).Returns(Task.CompletedTask);
            mockDbContext.Setup(db => db.Scripts.AnyAsync()).ReturnsAsync(false);

            var scope = new Mock<IServiceScope>();
            scope.Setup(s => s.ServiceProvider.GetService<KeyForgeDbContext>()).Returns(mockDbContext.Object);

            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            mockScopeFactory.Setup(f => f.CreateScope()).Returns(scope.Object);

            serviceProvider = new ServiceProviderWrapper(serviceProvider, mockScopeFactory.Object);

            // Act
            await serviceProvider.InitializeDatabaseAsync();

            // Assert
            mockDbContext.Verify(db => db.Database.MigrateAsync(), Times.Once);
        }

        #endregion

        #region 边界条件测试

        [Fact]
        public void AddInfrastructure_WithNullServices_ShouldThrowArgumentNullException()
        {
            // Arrange
            IServiceCollection nullServices = null;

            // Act & Assert
            var action = () => nullServices.AddInfrastructure(_mockConfiguration.Object);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddInfrastructure_WithNullConfiguration_ShouldThrowArgumentNullException()
        {
            // Arrange
            IConfiguration nullConfiguration = null;

            // Act & Assert
            var action = () => _services.AddInfrastructure(nullConfiguration);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ServiceRegistration_ShouldNotRegisterDuplicateServices()
        {
            // Arrange
            SetupConfiguration();

            // Act
            _services.AddInfrastructure(_mockConfiguration.Object);
            _services.AddInfrastructure(_mockConfiguration.Object); // 添加两次
            var serviceProvider = _services.BuildServiceProvider();

            // Assert
            // 验证服务只注册了一次
            var services = serviceProvider.GetServices<IScriptRepository>();
            services.Should().HaveCount(1);
        }

        #endregion

        #region 辅助方法

        private void SetupConfiguration()
        {
            _mockConfiguration.Setup(config => config.GetConnectionString("DefaultConnection"))
                .Returns("Data Source=test.db");

            // 设置所有配置节
            var mockSection = new Mock<IConfigurationSection>();
            _mockConfiguration.Setup(config => config.GetSection(It.IsAny<string>())).Returns(mockSection.Object);
        }

        #endregion

        #region 辅助类

        private class ServiceProviderWrapper : IServiceProvider
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly IServiceScopeFactory _scopeFactory;

            public ServiceProviderWrapper(IServiceProvider serviceProvider, IServiceScopeFactory scopeFactory)
            {
                _serviceProvider = serviceProvider;
                _scopeFactory = scopeFactory;
            }

            public object GetService(Type serviceType)
            {
                return _serviceProvider.GetService(serviceType);
            }

            public async Task InitializeDatabaseAsync()
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<KeyForgeDbContext>();
                await context.Database.MigrateAsync();
            }
        }

        #endregion
    }
}