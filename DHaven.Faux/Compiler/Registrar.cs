using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DHaven.Faux.Compiler
{
    internal class Registrar : IFauxRegistrar
    {
        private readonly IList<TypeInfo> services = new List<TypeInfo>();

        public IFauxRegistrar Register<TService>() where TService : class
        {
            services.Add(typeof(TService).GetTypeInfo());
            return this;
        }

        [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery", Justification = "Intentionally not providing reference.")]
        public IEnumerable<TypeInfo> GetRegisteredServices()
        {
            foreach (var info in services)
            {
                yield return info;
            }
        }
    }
}
