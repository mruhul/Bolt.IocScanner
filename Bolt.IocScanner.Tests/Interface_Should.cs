using Shouldly;
using Xunit;

namespace Bolt.IocScanner.Tests
{
    public class Interface_Should : IClassFixture<ServiceProviderFixture>
    {
        private readonly ServiceProviderFixture _fixture;

        public Interface_Should(ServiceProviderFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Not_Register()
        {
            var sut = _fixture.GetService<ISayNothing>();
            sut.ShouldBeNull();
        }
    }

    public interface ISayNothing { }
}
