# Bolt.IocScanner
A utlity library that Scan and bind your classes automatically to Ioc

## How I use this library?

Add a reference of latest nuget package of Bolt.IocScanner. In your Startup class add following lines

    public void ConfigureServices(IServiceCollection services)
    {
        services.Scan<Startup>();
    }

Now all classes in the same assembly of Startup class will be scanned and automatically bind to servicecollection. For example you create a new class in same project as below:

    public interface IBooksProxy
    {
        Task<Book> Get(string id);
    }
    
    public class BooksProxy : IBooksProxy
    {
        public Task<Book> Get(string id)
        {
            // your implementation here
        }
    }

You don't need to go th startup class and add code to register this proxy class to servicecollection. you can readily use this class in your controller or any other dependent classes. For example

    public class BooksController : Controller
    {
        private readonly IBooksProxy _proxy;

        public BooksController(IBooksProxy proxy)
        {
            _proxy = proxy;
        }
    }


## How can I bind a calss to self without using an inteface?

If a class doesn't implement any interface and the class is not abstract then the class bind to self as transient. See example below:

    public class BooksProxy
    {
        public Task<Book> Get(string id)
        {
            // your code here ...
        }
    }

    public class BooksController : Controller
    {
        private readonly BooksProxy _proxy;

        public BooksController(BooksProxy proxy)
        {
            _proxy = proxy;
        }
    }

## How I can control the lifecycle of the bindings?

By default all bindings use transient lifecycle. If you want different lifecycle then you need decorate your class with an attribute as below and can mention what lifecycle you like the class to bind.

    [AutoBind(LifeCycle.Scoped)]
    public class BooksProxy
    {

    }

    // or if you like to register as singleton

    [AutoBind(LifeCycle.Singleton)]
    public class ConnectionPool
    {

    }

## How I can exclude some classes from automatic binding?

Theres three ways you can do this.

### You can decorate the class that you want to exclude using an attribute

    [SkipAutoBind]
    public class ThisClassNotBind
    {

    }

### you can add the class type as exclude items of scan options as below

    public void ConfigureServices(IServiceCollection services)
    {
        services.Scan<Startup>(new IocScannerOptions
        {
            TypesToExclude = new[] { typeof(ThisClassNotBind) }
        });
    }

### you can tell in option that only bind when an "AutoBind" attribute provided. In that case any class in assemblies doesn't have "AutoBind" attribute will not be registered in ioc.

    public void ConfigureServices(IServiceCollection services)
    {
        services.Scan<Startup>(new IocScannerOptions
        {
            SkipWhenAutoBindMissing = true
        });
    }

## Does it bind to all implemented interfaces?

Yes if a class implement more than one interfaces the lib bind the class agains all interfaces. In case of Singleton and Scoped lifecycle all interface bind agains same instance. For example:

    [AutoBind(LifeCycle.Singleton)]
    public class Implementation : IProvideName, IProvidePrice
    {
    }

Now if you request for IProvideName and IProvidePrice both case the service provider will return same instance of Implementation throughout the whole life of the application. 

Same applies for Scoped lifecycle but in that case the behaviour is true for same scope. For new scope a new instance of Implementation will be provided by service provider.

## How can I ask to register using TryAdd instead

Sometimes you want to change the registration of your implementation when you setup Ioc in your test projects. In that case sometimes its easy to achieve if we use TryAdd instead of Add. So yes you can tell to use TryAdd instead of Add when register your implementation as below:

    [AutoBind(UseTryAdd = true)]
    public class BooksProxy
    {

    }



