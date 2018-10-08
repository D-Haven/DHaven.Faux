using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly IServiceCollection services;

        public FauxServiceProvider(IServiceCollection collection)
        {
            services = collection;
        }

        public object GetService(Type serviceType)
        {
            ServiceDescriptor descriptor = services.FirstOrDefault(d=> MatchedService(d, serviceType));

            return ResolveDescriptor(descriptor);
        }

        private bool MatchedService(ServiceDescriptor descriptor, Type requested)
        {
            bool matches = requested == descriptor.ServiceType;

            return matches;
        }

        private object ResolveDescriptor(ServiceDescriptor descriptor)
        {
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

            return constructor.Invoke(instances);
        }
    }
}
