using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Shouldly;
using Bolt.IocAttributes;
using System.Threading;

namespace Bolt.IocScanner.Tests
{
    public class AutoBind_Singleton_Should : IClassFixture<ServiceProviderFixture>
    {
        private readonly ServiceProviderFixture _fixture;

        public AutoBind_Singleton_Should(ServiceProviderFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Bind_Same_Instance_For_All_Interfaces()
        {
            var instanceA = _fixture.GetService<ISingletonInstanceA>();
            Thread.Sleep(5);
            var instanceB = _fixture.GetService<ISingletonInstanceB>();
            Thread.Sleep(5);
            var instanceC = _fixture.GetService<ISingletonInstanceC>();
            

            instanceA.Random.ShouldBe(instanceB.Random);
            instanceA.Random.ShouldBe(instanceC.Random);
            instanceB.Random.ShouldBe(instanceC.Random);
        }
    }

    public interface ISingletonInstanceA
    {
        string Random { get; }
    }
    public interface ISingletonInstanceB
    {
        string Random { get; }
    }
    public interface ISingletonInstanceC
    {
        string Random { get; }
    }


    [AutoBind(LifeCycle.Singleton)]
    public class SingleInstance : AbstractClass, ISingletonInstanceA, ISingletonInstanceB, ISingletonInstanceC
    {
    }
}
