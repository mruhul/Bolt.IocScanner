using Xunit;
using Shouldly;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System;
using Bolt.IocScanner.Attributes;

namespace Bolt.IocScanner.Tests
{
    public class AutoBind_Scoped_Should : IClassFixture<ServiceProviderFixture>
    {
        private readonly ServiceProviderFixture _fixture;

        public AutoBind_Scoped_Should(ServiceProviderFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Create_SingleInstance_Per_Scope()
        {
            ScopedHelloWorld instanceA;
            ScopedHelloWorld instanceB;
            ScopedHelloWorld instanceC;

            using (var sc = _fixture.ServiceProvider.CreateScope())
            {
                instanceA = sc.ServiceProvider.GetService<ScopedHelloWorld>();
                Thread.Sleep(5);
                instanceB = sc.ServiceProvider.GetService<ScopedHelloWorld>();
            }
            using (var sc = _fixture.ServiceProvider.CreateScope())
            {
                instanceC = sc.ServiceProvider.GetService<ScopedHelloWorld>();
            }

            instanceB.Random = "HelloWorld!";
            instanceC.Random = "HelloC";

            instanceA.Random.ShouldBe(instanceB.Random);
            instanceC.Random.ShouldBe("HelloC");
        }

        [Fact]
        public void Create_SameInstance_For_All_Interfaces()
        {
            IScopedInterfaceA instanceA;
            IScopedInterfaceB instanceB;
            IScopedInterfaceC instanceC;

            using (var sc = _fixture.ServiceProvider.CreateScope())
            {
                instanceA = sc.ServiceProvider.GetService<IScopedInterfaceA>();
                instanceB = sc.ServiceProvider.GetService<IScopedInterfaceB>();
                instanceC = sc.ServiceProvider.GetService<IScopedInterfaceC>();
            }

            instanceA.Random.ShouldBe(instanceB.Random);
            instanceA.Random.ShouldBe(instanceC.Random);
            instanceB.Random.ShouldBe(instanceC.Random);
        }
    }

    [AutoBind(LifeCycle.Scoped)]
    public class ScopedHelloWorld : AbstractClass
    {

    }

    public interface IScopedInterfaceA { string Random { get; } }
    public interface IScopedInterfaceB { string Random { get; } }
    public interface IScopedInterfaceC { string Random { get; } }

    [AutoBind(LifeCycle.Scoped)]
    public class ScopeWithMultipleInterfaces : AbstractClass, IScopedInterfaceA, IScopedInterfaceB, IScopedInterfaceC
    {
    }
}
