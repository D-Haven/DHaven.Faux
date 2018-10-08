using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DHaven.Faux.DependencyInjection
{
    /// <summary>
    /// Only for use with FauxCollection.
    /// </summary>
    internal class FauxServiceCollection : List<ServiceDescriptor>, IServiceCollection, ICollection<ServiceDescriptor>, IEnumerable<ServiceDescriptor>
    { }

    /// <summary>
    /// Only for use with FauxCollection.
    /// </summary>
    internal class FauxServiceProvider : IServiceProvider
    {
        private static readonly MethodInfo CreateLogger;
        private readonly IServiceCollection services;
        private readonly IDictionary<Type, object> SingletonObjects = new Dictionary<Type, object>();

        static FauxServiceProvider()
        {
            var type = typeof(LoggerFactoryExtensions);
            CreateLogger = type.GetMethods().FirstOrDefault(m => m.IsGenericMethod);
        }

        public FauxServiceProvider(IServiceCollection collection)
        {
            services = collection;
        }

        public object GetService(Type serviceType)
        {
            // Special: Microsoft.Extensions.Logging.ILogger`1[[??]]
            if (typeof(ILogger).IsAssignableFrom(serviceType))
            {
                // This is a logger.
                var logFactory = GetService(typeof(ILoggerFactory)) as ILoggerFactory;
                var typedCreate = CreateLogger.MakeGenericMethod(serviceType.GenericTypeArguments[0]);
                return typedCreate.Invoke(null, new object[] { logFactory });
            }
            // Special: Microsoft.Extensions.Options.IOptions`1[[??]]
            if (typeof(IOptions<>).IsAssignableFrom(serviceType))
            {
                //var optionsFactory = GetService(typeof(IOptionsFactory<>)) as IOptionsFactory<>;
                return null;
            }

            ServiceDescriptor descriptor = services.FirstOrDefault(d=> AreSame(d.ServiceType, serviceType));

            return ResolveDescriptor(descriptor);
        }


        private bool AreSame(Type one, Type two)
        {
            bool matches = one.IsGenericType == two.IsGenericType;
            matches = matches && one.GenericTypeArguments.Length == two.GenericTypeArguments.Length;

            for(int i = 0; i < one.GenericTypeArguments.Length; i++)
            {
                matches = matches && AreSame(one.GenericTypeArguments[i], two.GenericTypeArguments[i]);
            }

            return matches && one.Equals(two);
        }

        private object ResolveDescriptor(ServiceDescriptor descriptor)
        {
            if(descriptor == null)
            {
                return null;
            }

            if (descriptor.Lifetime == ServiceLifetime.Singleton
                && SingletonObjects.TryGetValue(descriptor.ServiceType, out var instance))
            {
                return instance;
            }

            if (descriptor.ImplementationInstance != null)
            {
                return descriptor.ImplementationInstance;
            }

            if (descriptor.ImplementationFactory != null)
            {
                return descriptor.ImplementationFactory(this);
            }

            var constructor = descriptor.ImplementationType.GetConstructors().FirstOrDefault();

            if (constructor == null)
            {
                return Activator.CreateInstance(descriptor.ImplementationType);
            }

            var parameters = constructor.GetParameters();
            var instances = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                instances[i] = GetService(parameters[i].ParameterType);
            }

            var value = constructor.Invoke(instances);

            if (descriptor.Lifetime == ServiceLifetime.Singleton
                     && !SingletonObjects.TryGetValue(descriptor.ServiceType, out instance))
            {
                SingletonObjects.Add(descriptor.ServiceType, value);
            }

            return value;
        }
    }
}
