using Microsoft.Extensions.DependencyInjection;

namespace Bolt.IocScanner
{
    public interface IServiceRegistry
    {
        void Register(IServiceCollection serviceCollection);
    }
}
