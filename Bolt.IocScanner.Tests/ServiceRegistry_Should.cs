using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using NSubstitute;
using Shouldly;

namespace Bolt.IocScanner.Tests
{
    public class ServiceRegistry_Should : IClassFixture<ServiceProviderFixture>
    {
        private readonly ServiceProviderFixture _fixture;

        public ServiceRegistry_Should(ServiceProviderFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Execute()
        {
            var service = _fixture.GetService<ISayHello>();

            service.ShouldNotBeNull();
        }

        [Fact]
        public void Not_Registered_In_ServiceCollection()
        {
            var registry = _fixture.GetService<IServiceRegistry>();

            registry.ShouldBeNull();
        }
    }

    public interface ISayHello
    {
        string Say { get; }
    }

    public class TestServiceRegistry : IServiceRegistry
    {
        public void Register(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ISayHello>(sp =>
            {
                var substitute = Substitute.For<ISayHello>();

                substitute.Say.Returns("Hello World");

                return substitute;
            });
        }
    }
}
