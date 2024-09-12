using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bolt.IocScanner.Attributes;
using Microsoft.Extensions.Configuration;

namespace Bolt.IocScanner
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Scane calling assembly and bind all classes in that assembly automatically to service collection based on attribute and convention
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        public static IServiceCollection Scan<T>(this IServiceCollection services, IocScannerOptions options)
        {
            return Scan(services, new[] { typeof(T).GetTypeInfo().Assembly }, options);
        }
        

        /// <summary>
        /// Scane calling assembly and autobind all classes in that assembly
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="options"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IServiceCollection Scan<T>(this IServiceCollection services, 
            IConfiguration configuration = null,
            IocScannerOptions options = null)
        {
            return ScanInternal(services, new[] { typeof(T).GetTypeInfo().Assembly }, configuration, options);
        }

        public static IServiceCollection Scan(this IServiceCollection services, 
            IEnumerable<Assembly> assemblies, 
            IocScannerOptions options)
        {
            new ScannerAndBinder(services, null).Run(assemblies.ToArray(), options);

            return services;
        }

        /// <summary>
        /// Scan supplied assemblies and bind them automatically to service collection based on attribute and convention
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblies"></param>
        /// <param name="configuration"></param>
        /// <param name="options"></param>
        public static IServiceCollection Scan(this IServiceCollection services, 
            IEnumerable<Assembly> assemblies, 
            IConfiguration configuration = null,
            IocScannerOptions options = null)
        {
            new ScannerAndBinder(services, configuration).Run(assemblies.ToArray(), options);

            return services;
        }
        
        private static IServiceCollection ScanInternal(this IServiceCollection services, 
            Assembly[] assemblies,
            IConfiguration configuration = null,
            IocScannerOptions options = null)
        {
            new ScannerAndBinder(services, configuration).Run(assemblies.ToArray(), options);

            return services;
        }

        
    }
}
