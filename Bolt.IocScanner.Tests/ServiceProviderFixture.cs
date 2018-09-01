using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Bolt.IocScanner.Tests
{
    [SkipAutoBind]
    public class ServiceProviderHelper
    {
        public ServiceProviderHelper(IEnumerable<Assembly> assemblies, IocScannerOptions options = null)
        {
            var sc = new ServiceCollection();

            sc.Scan(
                assemblies,
                options
            );

            ServiceProvider = sc.BuildServiceProvider();
        }

        public IServiceProvider ServiceProvider { get; set; }

        public T GetService<T>() => ServiceProvider.GetService<T>();
        public IEnumerable<T> GetServices<T>() => ServiceProvider.GetServices<T>();
    }

    [SkipAutoBind]
    public class ServiceProviderFixture
    {
        private readonly ServiceProviderHelper _sp;

        public ServiceProviderFixture()
        {
            _sp = new ServiceProviderHelper(new[] { typeof(HelloWorldSelfBind).Assembly }, new IocScannerOptions
            {
                TypesToExclude = new[] { typeof(ExcludeHelloWorldFromExcludeList) }
            });
        }

        public IServiceProvider ServiceProvider => _sp.ServiceProvider;

        public T GetService<T>() => _sp.GetService<T>();
        public IEnumerable<T> GetServices<T>() => _sp.GetServices<T>();
    }
}
