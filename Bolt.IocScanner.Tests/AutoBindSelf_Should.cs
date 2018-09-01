using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Shouldly;
using Bolt.IocAttributes;

namespace Bolt.IocScanner.Tests
{
    [SkipAutoBind]
    public class AutoBindSelf_Should : IClassFixture<ServiceProviderFixture>
    {
        private readonly ServiceProviderFixture _fixture;

        public AutoBindSelf_Should(ServiceProviderFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Bind_ToSelf()
        {
            var sut = _fixture.ServiceProvider.GetService<HelloWorldSelfBind>();
            sut.ShouldNotBeNull();
            sut.Hello().ShouldBe("Hello World!");
        }

        [Fact]
        public void Not_Bind_When_Class_Has_SkipAutoBind_Attribute()
        {
            var sut = _fixture.GetService<HelloWorldSkipAutoBind>();
            sut.ShouldBeNull();
        }
    }

    public class HelloWorldSelfBind
    {
        public string Hello()
        {
            return "Hello World!";
        }
    }

    [SkipAutoBind]
    public class HelloWorldSkipAutoBind
    {
        public string Hello()
        {
            return "Hello World!";
        }
    }
}
