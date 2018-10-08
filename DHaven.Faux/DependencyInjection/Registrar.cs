using System.Collections.Generic;
using System.Reflection;

namespace DHaven.Faux.DependencyInjection
{
    internal class Registrar : IFauxRegistrar
    {
        private readonly IList<TypeInfo> services = new List<TypeInfo>();

        public IFauxRegistrar Register<TService>() where TService : class
        {
            services.Add(typeof(TService).GetTypeInfo());
            return this;
        }

        public IEnumerable<TypeInfo> GetRegisteredServices()
        {
            foreach (var info in services)
            {
                yield return info;
            }
        }
    }
}
