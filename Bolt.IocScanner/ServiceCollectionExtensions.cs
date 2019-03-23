using Bolt.IocAttributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bolt.IocScanner
{
    public class IocScannerOptions
    {
        internal List<Func<Type,bool>> Excludes { get; set; }

        public IocScannerOptions Exclude(Func<Type, bool> exlclude)
        {
            if (Excludes == null) Excludes = new List<Func<Type, bool>>();

            Excludes.Add(exlclude);

            return this;
        }

        public IEnumerable<Type> TypesToExclude { get; set; }
        public bool SkipWhenAutoBindMissing { get; set; }
    }

    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Scane calling assembly and bind all classes in that assembly automatically to service collection based on attribute and convention
        /// </summary>
        /// <param name="source"></param>
        /// <param name="options"></param>
        public static IServiceCollection Scan<T>(this IServiceCollection source, IocScannerOptions options)
        {
            return Scan(source, new[] { typeof(T).GetTypeInfo().Assembly }, options);
        }

        /// <summary>
        /// Scane calling assembly and autobind all classes in that assembly
        /// </summary>
        /// <param name="source"></param>
        public static IServiceCollection Scan<T>(this IServiceCollection source)
        {
            return Scan(source, new[] { typeof(T).GetTypeInfo().Assembly }, null);
        }

        /// <summary>
        /// Scan supplied assemblies and bind them automatically to service collection based on attribute and convention
        /// </summary>
        /// <param name="source"></param>
        /// <param name="assemblies"></param>
        public static IServiceCollection Scan(this IServiceCollection source, IEnumerable<Assembly> assemblies)
        {
            return Scan(source, assemblies, null);
        }


        /// <summary>
        /// Scan supplied assemblies and bind them automatically to service collection based on attribute and convention
        /// </summary>
        /// <param name="source"></param>
        /// <param name="assemblies"></param>
        public static IServiceCollection Scan(this IServiceCollection source, IEnumerable<Assembly> assemblies, IocScannerOptions options)
        {
            options = options ?? new IocScannerOptions();

            var skipAutoBindAttributeFullName = typeof(SkipAutoBindAttribute).FullName;
            var autoBindAttributeFullName = typeof(AutoBindAttribute).FullName;

            if(options.TypesToExclude?.Any() ?? false)
            {
                options.Exclude(t => options.TypesToExclude.Any(e => e.FullName == t.FullName));
            }

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    var typeInfo = type.GetTypeInfo();

                    var isAbstract = typeInfo.IsAbstract || typeInfo.IsInterface;

                    if (isAbstract) continue;

                    var attributes = typeInfo.GetCustomAttributes();

                    if (attributes.Any(a => a.GetType().FullName == skipAutoBindAttributeFullName)) continue;

                    if (ShouldExclude(type, options)) continue;

                    var autoBindAttribute = attributes.FirstOrDefault(x => x.GetType().FullName == autoBindAttributeFullName) as AutoBindAttribute;

                    if (autoBindAttribute == null && options.SkipWhenAutoBindMissing) continue;
                    
                    var interfaces = typeInfo.GetInterfaces();

                    if (!interfaces.Any())
                    {
                        if (autoBindAttribute != null)
                        {
                            BindToSelf(source, type, autoBindAttribute);
                        }

                        continue;
                    }

                    if (interfaces.Any(x => x == typeof(IServiceRegistry)))
                    {
                        var registry = Activator.CreateInstance(type) as IServiceRegistry;

                        registry.Register(source);

                        continue;
                    }

                    if (autoBindAttribute == null)
                    {
                        autoBindAttribute = new AutoBindAttribute(LifeCycle.Transient) { UseTryAdd = false };
                    }

                    BindToIntefaces(source, type, interfaces, autoBindAttribute);
                }
            }

            return source;
        }

        private static bool ShouldExclude(Type type, IocScannerOptions options)
        {
            if (options.Excludes != null)
            {
                foreach (var func in options.Excludes)
                {
                    if (func.Invoke(type)) return true;
                }
            }

            return false;
        }

        private static void BindToIntefaces(IServiceCollection source, Type type, Type[] interfaces, AutoBindAttribute attr)
        {
            switch (attr.LifeCycle)
            {
                case LifeCycle.Transient:
                    {
                        if (attr.UseTryAdd)
                        {
                            foreach (var interfaceImplemented in interfaces)
                            {
                                source.TryAdd(ServiceDescriptor.Transient(interfaceImplemented, type));
                            }
                        }
                        else
                        {
                            foreach (var interfaceImplemented in interfaces)
                            {
                                source.Add(ServiceDescriptor.Transient(interfaceImplemented, type));
                            }
                        }

                        break;
                    }
                case LifeCycle.Scoped:
                    {
                        if (attr.UseTryAdd)
                        {
                            TryBindAsScoped(source, type, interfaces, attr);
                        }
                        else
                        {
                            BindAsScoped(source, type, interfaces, attr);
                        }

                        break;
                    }
                case LifeCycle.Singleton:
                    {
                        if (attr.UseTryAdd)
                        {
                            TryBindAsSingleton(source, type, interfaces, attr);
                        }
                        else
                        {
                            BindAsSingleton(source, type, interfaces, attr);
                        }

                        break;
                    }
                default:
                    {
                        throw new Exception($"Unsupported LifcycleType {attr.LifeCycle} provided");
                    }
            }
        }

        private static void TryBindAsSingleton(IServiceCollection source, Type type, Type[] interfaces, AutoBindAttribute attr)
        {
            if (interfaces.Length > 1)
            {
                var firstInterface = interfaces[0];

                source.TryAdd(ServiceDescriptor.Singleton(firstInterface, type));

                for (var i = 1; i < interfaces.Length; i++)
                {
                    source.TryAdd(ServiceDescriptor.Singleton(interfaces[i], sp => sp.GetService(firstInterface)));
                }
            }
            else
            {
                foreach (var interfaceImplemented in interfaces)
                {
                    source.TryAdd(ServiceDescriptor.Singleton(interfaceImplemented, type));
                }
            }
        }

        private static void BindAsSingleton(IServiceCollection source, Type type, Type[] interfaces, AutoBindAttribute attr)
        {
            if (interfaces.Length > 1)
            {
                var firstInterface = interfaces[0];

                source.Add(ServiceDescriptor.Singleton(firstInterface, type));

                for (var i = 1; i < interfaces.Length; i++)
                {
                    source.Add(ServiceDescriptor.Singleton(interfaces[i], sp => sp.GetService(firstInterface)));
                }
            }
            else
            {
                foreach (var interfaceImplemented in interfaces)
                {
                    source.Add(ServiceDescriptor.Singleton(interfaceImplemented, type));
                }
            }
        }


        private static void TryBindAsScoped(IServiceCollection source, Type type, Type[] interfaces, AutoBindAttribute attr)
        {
            if (interfaces.Length > 1)
            {
                var firstInterface = interfaces[0];

                source.TryAdd(ServiceDescriptor.Scoped(firstInterface, type));

                for (var i = 1; i < interfaces.Length; i++)
                {
                    source.TryAdd(ServiceDescriptor.Scoped(interfaces[i], sp => sp.GetService(firstInterface)));
                }
            }
            else
            {
                foreach (var interfaceImplemented in interfaces)
                {
                    source.TryAdd(ServiceDescriptor.Scoped(interfaceImplemented, type));
                }
            }
        }

        private static void BindAsScoped(IServiceCollection source, Type type, Type[] interfaces, AutoBindAttribute attr)
        {
            if (interfaces.Length > 1)
            {
                var firstInterface = interfaces[0];

                source.Add(ServiceDescriptor.Scoped(firstInterface, type));

                for (var i = 1; i < interfaces.Length; i++)
                {
                    source.Add(ServiceDescriptor.Scoped(interfaces[i], sp => sp.GetService(firstInterface)));
                }
            }
            else
            {
                foreach (var interfaceImplemented in interfaces)
                {
                    source.Add(ServiceDescriptor.Scoped(interfaceImplemented, type));
                }
            }
        }


        private static void BindToSelf(IServiceCollection source, Type type, AutoBindAttribute attr)
        {
            switch (attr.LifeCycle)
            {
                case LifeCycle.Transient:
                    {
                        if (attr.UseTryAdd)
                        {
                            source.TryAddTransient(type);
                        }
                        else
                        {
                            source.AddTransient(type);
                        }

                        break;
                    }
                case LifeCycle.Scoped:
                    {
                        if (attr.UseTryAdd)
                        {
                            source.TryAddScoped(type);
                        }
                        else
                        {
                            source.AddScoped(type);
                        }

                        break;
                    }
                case LifeCycle.Singleton:
                    {
                        if (attr.UseTryAdd)
                        {
                            source.TryAddSingleton(type);
                        }
                        else
                        {
                            source.AddSingleton(type);
                        }

                        break;
                    }
                default:
                    {
                        throw new Exception($"Unsupported LifcycleType {attr.LifeCycle} provided");
                    }
            }
        }
    }
}
