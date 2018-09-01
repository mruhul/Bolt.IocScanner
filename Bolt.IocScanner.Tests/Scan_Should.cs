using Xunit;
using Shouldly;

namespace Bolt.IocScanner.Tests
{
    [SkipAutoBind]
    public class Scan_Should : IClassFixture<ServiceProviderFixture>
    {
        private readonly ServiceProviderFixture _fixture;

        public Scan_Should(ServiceProviderFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Exclude_When_Pass_Type_In_ExcludeList()
        {
            var sut = _fixture.GetService<ExcludeHelloWorldFromExcludeList>();
            sut.ShouldBeNull();
        }

        [Fact]
        public void Ignore_Classes_That_Doesnt_Have_AutoBind_Attribute_When_SkipAutoBindWhenAttributesMissing_Is_True()
        {
            var sp = new ServiceProviderHelper(new[] { typeof(IgnoreHelloWorldWithoutAutoBind).Assembly }, new IocScannerOptions
            {
                SkipAutoBindWhenAttributesMissing = true
            });

            var sut = sp.GetService<IgnoreHelloWorldWithoutAutoBind>();
            sut.ShouldBeNull();
        }

        [Fact]
        public void Ignore_Abstract_Classes()
        {
            var abstractHelloWorld = _fixture.GetService<AbstractHelloWorld>();
            abstractHelloWorld.ShouldBeNull();
        }

        [Fact]
        public void Bind_All_Interfaces()
        {
            _fixture.GetService<IHelloWorldForAbstract>().ShouldNotBeNull();
            _fixture.GetService<IHelloWorldSecondInterface>().ShouldNotBeNull();
            _fixture.GetService<IHelloWorldThirdInterface>().ShouldNotBeNull();
        }
    }

    public interface IHelloWorldForAbstract
    {
    }

    public interface IHelloWorldSecondInterface
    {
    }

    public interface IHelloWorldThirdInterface { }

    public class MultipleInterfaceHelloWorld : AbstractHelloWorld, IHelloWorldSecondInterface, IHelloWorldThirdInterface
    {

    }

    public abstract class AbstractHelloWorld : IHelloWorldForAbstract
    {

    }

    public static class StaticHelloWorld
    {

    }

    public class ExcludeHelloWorldFromExcludeList
    {
        public string Hello() => "Hello World!";
    }

    public class IgnoreHelloWorldWithoutAutoBind
    {
        public string Hello() => "Hello World!";
    }
}
