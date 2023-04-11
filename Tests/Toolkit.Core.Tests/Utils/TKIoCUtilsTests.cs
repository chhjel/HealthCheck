using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Web.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;
using Xunit.Abstractions;

namespace QoDL.Toolkit.Core.Tests.Extensions
{
    public class TKIoCUtilsTests
    {
        public ITestOutputHelper Output { get; }

        public TKIoCUtilsTests(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void GetInstance_WithScopedService_ReturnsInstance()
        {
            ConfigureServiceProvider();
            var instance = TKIoCUtils.GetInstance<IScopedService>();
            Assert.NotNull(instance);
        }

        [Fact]
        public void GetInstance_WithTransientService_ReturnsInstance()
        {
            ConfigureServiceProvider();
            var instance = TKIoCUtils.GetInstance<ITransientService>();
            Assert.NotNull(instance);
        }

        [Fact]
        public void GetInstance_WithSingletonService_ReturnsInstance()
        {
            ConfigureServiceProvider();
            var instance = TKIoCUtils.GetInstance<ISingletonService>();
            Assert.NotNull(instance);
        }

        private static void ConfigureServiceProvider()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<IScopedService, ScopedService>();
            serviceCollection.AddTransient<ITransientService, TransientService>();
            serviceCollection.AddSingleton<ISingletonService, SingletonService>();

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
            TKIoCSetup.ConfigureForServiceProvider(serviceProvider);
        }
    }

    public interface IScopedService { }
    public class ScopedService : IScopedService { }
    public interface ITransientService { }
    public class TransientService : ITransientService { }
    public interface ISingletonService { }
    public class SingletonService : ISingletonService { }
}
