using Bolt.IocScanner.Attributes;
using Xunit;
using Shouldly;

namespace Bolt.IocScanner.Tests
{
    [SkipAutoBind]
    public class AutoBindTransient_Should : IClassFixture<ServiceProviderFixture>
    {
        private readonly ServiceProviderFixture _fixture;

        public AutoBindTransient_Should(ServiceProviderFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Bind_As_Transient()
        {
            var sut = _fixture.GetService<ITransientHelloWorld>();

            sut.ShouldNotBeNull();
            sut.Hello().ShouldBe("Hello World!");
        }
    }

    public interface ITransientHelloWorld
    {
        string Hello();
    }

    [AutoBind]
    public class TransientHelloWorld : ITransientHelloWorld
    {
        public string Hello()
        {
            return "Hello World!";
        }
    }
}
