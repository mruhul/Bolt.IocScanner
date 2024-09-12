using System;
using System.Linq;
using System.Reflection;
using Bolt.IocScanner.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bolt.IocScanner;

public class ScannerAndBinder
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;

    public ScannerAndBinder(
        IServiceCollection services, 
        IConfiguration configuration = null)
    {
        _services = services;
        _configuration = configuration;
    }

    public void Run(Assembly[] assemblies, IocScannerOptions options)
    {
        foreach (var assembly in assemblies)
        {
            Run(assembly, options);
        }
    }
    
    private void Run(Assembly assembly, IocScannerOptions options)
    {
        var types = assembly.GetTypes()
            .Where(x => x is { IsAbstract: false, IsInterface: false, IsClass: true });

        foreach (var type in types)
        {
            BindService(type, options);
            BindService(type);
            Configure(type);
        }
    }

    private void BindService(Type type)
    {
        if (type.GetInterfaces().Any(x => x == typeof(IServiceRegistry)))
        {
            if (Activator.CreateInstance(type) is IServiceRegistry service)
            {
                service.Register(_services);
            }
        }
    }
    
    private void BindService(Type type, IocScannerOptions options)
    {
        var skipAttr = type.GetCustomAttribute<SkipAutoBindAttribute>();
        
        if(skipAttr != null) return;
        
        var attr = type.GetCustomAttribute<AutoBindAttribute>();
        var isMatch = options.BindServicesOnMatch?.Invoke(type) ?? false;
        
        if(isMatch == false && attr == null) return;

        attr ??= new AutoBindAttribute();

        var isTypeGeneric = type.IsGenericType;
        var typeInterfaces = type.GetInterfaces()
            .Where(inf => !options.InterfacesToExclude.Contains(inf))
            .ToArray();

        var implType = isTypeGeneric ? type.GetGenericTypeDefinition() : type;

        if (typeInterfaces.Length == 0)
        {
            switch (attr.LifeCycle)
            {
                case LifeCycle.Scoped:
                    _services.TryAddScoped(implType);
                    break;
                case LifeCycle.Singleton:
                    _services.TryAddSingleton(implType);
                    break;
                default:
                    _services.TryAddTransient(implType);
                    break;
            }
            
            return;
        }

        if (typeInterfaces.Length == 1 || attr.LifeCycle == LifeCycle.Transient)
        {
            foreach (var typeInterface in typeInterfaces)
            {
                var appliedInterfaceType = isTypeGeneric
                        ? typeInterface.GetGenericTypeDefinition()
                        : typeInterface;
                
                switch (attr.LifeCycle)
                {
                    case LifeCycle.Scoped:
                        _services.TryAddEnumerable(ServiceDescriptor.Scoped(appliedInterfaceType, implType));
                        break;
                    case LifeCycle.Singleton:
                        _services.TryAddEnumerable(ServiceDescriptor.Singleton(appliedInterfaceType, implType));
                        break;
                    default:
                        _services.TryAddEnumerable(ServiceDescriptor.Transient(appliedInterfaceType, implType));
                        break;
                }
            }
        }
        else
        {
            var rootInterface = typeInterfaces[0];
            var appliedRootInterfaceType = isTypeGeneric
                ? rootInterface.GetGenericTypeDefinition()
                : rootInterface;
            
            switch (attr.LifeCycle)
            {
                case LifeCycle.Scoped:
                    _services.TryAddEnumerable(ServiceDescriptor.Scoped(appliedRootInterfaceType, implType));
                    break;
                case LifeCycle.Singleton:
                    _services.TryAddEnumerable(ServiceDescriptor.Singleton(appliedRootInterfaceType, implType));
                    break;
                default:
                    _services.TryAddEnumerable(ServiceDescriptor.Transient(appliedRootInterfaceType, implType));
                    break;
            }

            for (int i = 1; i < typeInterfaces.Length; i++)
            {
                var currentInterface = typeInterfaces[i];
                var currentInterfaceType = isTypeGeneric 
                        ? currentInterface.GetGenericTypeDefinition() 
                        : currentInterface;
                
                switch (attr.LifeCycle)
                {
                    case LifeCycle.Scoped:
                        _services.TryAdd(ServiceDescriptor.Scoped(currentInterfaceType, sp => sp.GetRequiredService(appliedRootInterfaceType)));
                        break;
                    case LifeCycle.Singleton:
                        _services.TryAdd(ServiceDescriptor.Singleton(currentInterfaceType, sp => sp.GetRequiredService(appliedRootInterfaceType)));
                        break;
                    default:
                        _services.TryAdd(ServiceDescriptor.Transient(currentInterfaceType, sp => sp.GetRequiredService(appliedRootInterfaceType)));
                        break;
                }
            }
        }
        
    }
    
    private void Configure(Type type)
    {
        if(_configuration == null) return;
        
        var att = type.GetCustomAttribute<BindFromConfigAttribute>();
        
        if(att == null) return;

        var sectionName = string.IsNullOrWhiteSpace(att.SectionName) 
                            ? $"App:{type.Name}" 
                            : att.SectionName;
        
        var section = att.IsOptional 
            ? _configuration.GetSection(sectionName)
            : _configuration.GetRequiredSection(sectionName);

        if (!section.Exists())
        {
            return;
        }

        // Use reflection to call services.Configure<TOptions>(section)
        var configureMethod = typeof(OptionsConfigurationServiceCollectionExtensions)
            .GetMethod(nameof(OptionsConfigurationServiceCollectionExtensions.Configure),
                [typeof(IServiceCollection), typeof(IConfiguration)]);

        var genericConfigureMethod = configureMethod?.MakeGenericMethod(type);

        genericConfigureMethod?.Invoke(null, [_services, section]);
    }
}