# Bolt.IocScanner
A utility library that scan and bind your classes automatically to IOC based
on Attribute and also can bind strong type data as IOptions<T> from configuration.

## How I use this library?

Add a reference of latest nuget package of Bolt.IocScanner. In your Startup class add following lines

    builder.Services.Scan<Startup>();
    // Or if you want to bind options from configuration pas configuration
    builser.Services.Scan<Startup>(builder.Configuration)
    

Now all classes in the same assembly of Startup class will be scanned and automatically bind to servicecollection. For example you create a new class in same project as below:

    public interface IBooksProxy
    {
        Task<Book> Get(string id);
    }
    
    [AutoBind]
    public class BooksProxy : IBooksProxy
    {
        public Task<Book> Get(string id)
        {
            // your implementation here
        }
    }

You don't need to open the startup class and add code to register this proxy class to servicecollection. you can readily use this class in your controller or any other dependent classes. For example

    public class BooksController : Controller
    {
        private readonly IBooksProxy _proxy;

        public BooksController(IBooksProxy proxy)
        {
            _proxy = proxy;
        }
    }


## Can I scan mutiple assemblies?

Yes you can. Here is an example:
    
    builder.Services.Scan(new []{
            typeof(BookStore.Web.Startup).Assembly,
            typeof(BookStore.Infrastructure.ISerializer).Assembly
        }, build.Configuration);

## How can I bind a class to self without using an inteface?

Just add `AutoBind` attribute to that class. When a class doesn't have any interface then it bind to self:

    [AutoBind]
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

## Can I bind classes automatically based on convention?

Yes you can. Here is an example where all classes matches the criteria 
will bind automatically without the need to add `AutoBind` attribute.

```csharp
    
    builder.Services.Scan<Startup>(new IocScannerOptions
    {
        BindServicesOnMatch = (type) => ["Provider","Service"].Any(x => type.Name.EndsWith(x))    
    });
```


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

Theres four ways you can do this.

### You can decorate the class that you want to exclude using an attribute

    [SkipAutoBind]
    public class ThisClassNotBind
    {

    }

### exclude some interfaces to bind against.

    // By default we exclude IDispoable interface
    services.Scan<Startup>(new IocScannerOptions
        {
            InterfacesToExclude = new[] { typeof(ThisClassNotBind) }
        });

### you can tell in option that only bind when an "AutoBind" attribute provided. In that case any class in assemblies doesn't have "AutoBind" attribute will not be registered in ioc.

    public void ConfigureServices(IServiceCollection services)
    {
        services.Scan<Startup>(new IocScannerOptions
        {
            SkipWhenAutoBindMissing = true
        });
    }

### you can even pass your own logic to exclude any types using a func as below

    public void ConfigureServices(IServiceCollection services)
    {
        services.Scan<Startup>(new IocScannerOptions()
            .Exclude(t => t.Name.Equals("Startup")));
    }


## Does it bind to all implemented interfaces?

Yes if a class implement more than one interfaces the lib bind the class against all interfaces. In case of Singleton and Scoped lifecycle all interface bind against same instance unless
any interface provided in `InterfacesToExclude` property of `IocScannerOptions`. For example:

    [AutoBind(LifeCycle.Singleton)]
    public class Implementation : IProvideName, IProvidePrice
    {
    }

Now if you request for IProvideName and IProvidePrice both case the service provider will return same instance of Implementation throughout the whole life of the application. 

Same applies for Scoped lifecycle but in that case the behaviour is true for same scope. For new scope a new instance of Implementation will be provided by service provider.


## I want to control how to bind but define in different area of the project

The library scan for `IServiceRegistry` interface and execute them to allow you to bind by yourself.

    public class SampleRegistration : IServiceRegistry
    {
        public void Register(IServiceCollection serviceCollection)
        {
            serviceCollection.Bind(typeof(IDataProvider<,>),typeof(DefaultDataProvider<,>));
        }
    } 

# Auto configure Settings from configurtion

### How I can configure settings automatically from configuration

Define you settings DTO and decorate with attribute `BindFromConfig`

```csharp
    [BindFromConfig]
    public class TenantSettings
    {
        public string Theme {get; set; }
    }    
```

Now make sure you define the settings in `appsettings.config` or your configuration providers.

```json
    {
      "App": {
        "TenantSettings": {
          "Theme": "dark"
        }
      }
    } 
```

You should able to use this settings as below:

```csharp
public class ColorPicker(IOptions<TenantSettings> options)
{
    public string TextColor()
    {
        if(options.Value.Theme == "dark") return "white";
        return "black";
    }
}
```

### Can i define from which section settings should load?

Yes you can. See example below:

```csharp
    [BindFromConfig("Bolt:Tenants:Settings", isOptional: false)]
    public class TenantSettings
    {
        public string Theme {get; set; }
    }  
```

Now the system will load the settings from defined section name in attribute.

```json
{
  "Bolt": {
    "Tenants": {
      "Settings": {
        "Theme": "dark"
      }
    }
  }
} 
```

## Can I make settings optional?

Yes you can. Just pass `isOptional` to `true` as below:

```csharp
    [BindFromConfig("Bolt:Tenants:Settings", isOptional: true)]
    public class TenantSettings
    {
        public string Theme {get; set; }
    }  
```