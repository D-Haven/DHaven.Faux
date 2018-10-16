using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DHaven.Faux.Compiler
{
    public class FauxDiscovery
    {
        private readonly Task<IDictionary<Type,Type>> fauxMapping;
        private readonly ICollection<Type> unregistered = new Collection<Type>();
        private readonly ISet<string> references = new HashSet<string>();
        private readonly ISet<Assembly> assemblyReferences = new HashSet<Assembly>();
        private readonly IDictionary<TypeInfo,string> generatedTypes = new Dictionary<TypeInfo, string>();

        internal FauxDiscovery(Assembly startingAssembly)
        {
            fauxMapping = Task.Run(
                (Func<IDictionary<Type,Type>>)(() =>
            {
                var types = new Dictionary<Type,Type>();

                DiscoverMappings(startingAssembly, types);
                
                return types;
            }));
        }

        public async Task<IEnumerable<TypeInfo>> GetAllFauxInterfaces()
        {
            return (await fauxMapping).Keys.Select(t => t.GetTypeInfo())
                .Union(unregistered.Select(t => t.GetTypeInfo()));
        }

        public async Task<string> GetImplementationNameFor(TypeInfo fauxInterface)
        {
            (await fauxMapping).TryGetValue(fauxInterface.AsType(), out var implementation);
            var className = implementation?.FullName;

            if (className == null) generatedTypes.TryGetValue(fauxInterface, out className);

            return className;
        }

        public async Task<IEnumerable<string>> GetReferenceLocations()
        {
            await fauxMapping;

            return references;
        }

        public async Task<IEnumerable<Assembly>> GetReferenceAssemblies()
        {
            await fauxMapping;

            return assemblyReferences;
        }

        public void RegisterType(TypeInfo fauxInterface, string fullyQualifiedName)
        {
            generatedTypes.Add(fauxInterface, fullyQualifiedName);
        }

        private void DiscoverMappings(Assembly assembly, IDictionary<Type,Type> types)
        {
            var referenceLocation = assembly.Location;

            if (references.Contains(referenceLocation))
            {
                return;
            }
  
            references.Add(referenceLocation);
            assemblyReferences.Add(assembly);

            if (!(assembly.FullName.StartsWith("System.")
                  || assembly.FullName.StartsWith("FauGen")
                  || assembly.FullName.StartsWith("Microsoft.Extensions.")))
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (IsFauxInterface(type) && !types.ContainsKey(type))
                    {
                        unregistered.Add(type.GetTypeInfo());
                    }
                    else
                    {
                        var faux = type.GetInterfaces().FirstOrDefault(IsFauxInterface);
                        if (faux == null) continue;

                        types.Add(faux, type);

                        if (unregistered.Contains(faux))
                        {
                            // If we found the interface before the implementation, add it here.
                            unregistered.Remove(faux);
                        }
                    }
                }
            }

            foreach (var dependency in assembly.GetReferencedAssemblies())
            {
                try
                {
                    DiscoverMappings(Assembly.Load(dependency), types);
                }
                catch (Exception)
                {
                    // This is just a first ditch effort.  Don't go to great lengths for loading assemblies.
                }
            }
        }

        private static bool IsFauxInterface(Type type)
        {
            return type.IsInterface && type.GetCustomAttribute<FauxClientAttribute>() != null;
        }
    }
}