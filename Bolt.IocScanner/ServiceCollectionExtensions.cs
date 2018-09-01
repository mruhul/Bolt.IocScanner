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
        public IEnumerable<Type> TypesToExclude { get; set; }
        public bool BindAsTransientWhenAttributeMissing { get; set; }
    }

    public static class ServiceCollectionExtensions
    {
#if !NETSTANDARD1_6
        /// <summary>
        /// Scane calling assembly and bind all classes in that assembly automatically to service collection based on attribute and convention
        /// </summary>
        /// <param name="source"></param>
        /// <param name="options"></param>
        public static void Scan(this IServiceCollection source, IocScannerOptions options)
        {
            Scan(source, new[] { Assembly.GetCallingAssembly() }, null);
        }

        /// <summary>
        /// Scane calling assembly and autobind all classes in that assembly
        /// </summary>
        /// <param name="source"></param>
        public static void Scan(this IServiceCollection source)
        {
            Scan(source, new[] { Assembly.GetCallingAssembly() }, null);
        }
#endif

        /// <summary>
        /// Scan supplied assemblies and bind them automatically to service collection based on attribute and convention
        /// </summary>
        /// <param name="source"></param>
        /// <param name="assemblies"></param>
        public static void Scan(this IServiceCollection source, IEnumerable<Assembly> assemblies)
        {
            Scan(source, assemblies, null);
        }


        /// <summary>
        /// Scan supplied assemblies and bind them automatically to service collection based on attribute and convention
        /// </summary>
        /// <param name="source"></param>
        /// <param name="assemblies"></param>
        public static void Scan(this IServiceCollection source, IEnumerable<Assembly> assemblies, IocScannerOptions options)
        {
            options = options ?? new IocScannerOptions();

            var skipAutoBindAttributeFullName = typeof(SkipAutoBindAttribute).FullName;
            var autoBindAttributeFullName = typeof(AutoBindAttribute).FullName;

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    var typeInfo = type.GetTypeInfo();

                    var isAbstract = typeInfo.IsAbstract;

                    if (isAbstract) continue;

                    var attributes = typeInfo.GetCustomAttributes();

                    if (attributes.Any(a => a.GetType().FullName == skipAutoBindAttributeFullName)) continue;

                    if (options.TypesToExclude?.Any(t => t.FullName == type.FullName) ?? false) continue;

                    var autoBindAttribute = attributes.FirstOrDefault(x => x.GetType().FullName == autoBindAttributeFullName) as AutoBindAttribute;

                    if (autoBindAttribute == null && !options.BindAsTransientWhenAttributeMissing) continue;

                    var useTryAdd = autoBindAttribute?.UseTryAdd ?? false;
                    var lifeCycle = autoBindAttribute?.LifeCycle ?? LifeCycle.Transient;

                    var interfaces = typeInfo.GetInterfaces();

                    if (!interfaces.Any())
                    {
                        BindToSelf(source, type, lifeCycle, useTryAdd);

                        continue;
                    }

                    BindToIntefaces(source, type, interfaces, lifeCycle, useTryAdd);
                }
            }
        }

        private static void BindToIntefaces(IServiceCollection source, Type type, Type[] interfaces, LifeCycle lifeCycle, bool useTryAdd)
        {
            switch (lifeCycle)
            {
                case LifeCycle.Transient:
                    {
                        if (useTryAdd)
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
                        if (useTryAdd)
                        {
                            foreach (var interfaceImplemented in interfaces)
                            {
                                source.TryAdd(ServiceDescriptor.Scoped(interfaceImplemented, type));
                            }
                        }
                        else
                        {
                            foreach (var interfaceImplemented in interfaces)
                            {
                                source.Add(ServiceDescriptor.Scoped(interfaceImplemented, type));
                            }
                        }

                        break;
                    }
                case LifeCycle.Singleton:
                    {
                        if (useTryAdd)
                        {
                            foreach (var interfaceImplemented in interfaces)
                            {
                                source.TryAdd(ServiceDescriptor.Singleton(interfaceImplemented, type));
                            }
                        }
                        else
                        {
                            foreach (var interfaceImplemented in interfaces)
                            {
                                source.Add(ServiceDescriptor.Singleton(interfaceImplemented, type));
                            }
                        }

                        break;
                    }
                default:
                    {
                        throw new Exception($"Unsupported LifcycleType {lifeCycle} provided");
                    }
            }
        }

        private static void BindToSelf(IServiceCollection source, Type type, LifeCycle lifeCycle, bool useTryAdd)
        {
            switch (lifeCycle)
            {
                case LifeCycle.Transient:
                    {
                        if (useTryAdd)
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
                        if (useTryAdd)
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
                        if (useTryAdd)
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
                        throw new Exception($"Unsupported LifcycleType {lifeCycle} provided");
                    }
            }
        }
    }
}
