using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DHaven.Faux.Compiler
{
    public class FauxDiscovery
    {
        private readonly Task<IDictionary<Type,Type>> fauxMapping;
        private readonly ICollection<Type> unregistered = new Collection<Type>();
        private readonly IDictionary<string,Assembly> references = new Dictionary<string,Assembly>();
        private readonly IDictionary<TypeInfo,string> generatedTypes = new Dictionary<TypeInfo, string>();
        private readonly ILogger<FauxDiscovery> logger;

        internal FauxDiscovery(Assembly startingAssembly, ILogger<FauxDiscovery> log)
        {
            logger = log;
            fauxMapping = Task.Run(
                (Func<IDictionary<Type,Type>>)(() =>
            {
                var types = new Dictionary<Type,Type>();

                logger.LogDebug($"Finding all [FauxClient] interfaces starting from {startingAssembly.FullName}");
                DiscoverMappings(startingAssembly, types);

                if (types.Count + unregistered.Count == 0)
                {
                    logger.LogWarning($"Unable to find any [FauxClient] interfaces in {startingAssembly.FullName} or it's dependencies");
                }
                
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

            return references.Values.Select(dependency => dependency.Location);
        }

        public async Task<IEnumerable<Assembly>> GetReferenceAssemblies()
        {
            await fauxMapping;

            return references.Values;
        }

        public void RegisterType(TypeInfo fauxInterface, string fullyQualifiedName)
        {
            generatedTypes.Add(fauxInterface, fullyQualifiedName);
        }

        private void DiscoverMappings(Assembly assembly, IDictionary<Type,Type> types)
        {
            if (references.ContainsKey(assembly.FullName))
            {
                return;
            }
  
            references.Add(assembly.FullName, assembly);

            if (!(assembly.FullName.StartsWith("System.")
                  || assembly.FullName.StartsWith("FauxGen")
                  || assembly.FullName.StartsWith("Microsoft.Extensions.")))
            {
                logger.LogTrace($"Inspecting {assembly.FullName}");

                foreach (var type in assembly.GetTypes())
                {
                    if (IsFauxInterface(type) && !types.ContainsKey(type))
                    {
                        logger.LogTrace($"Found interface {type.FullName}, marked as unregistered.");
                        unregistered.Add(type.GetTypeInfo());
                    }
                    else
                    {
                        var faux = type.GetInterfaces().FirstOrDefault(IsFauxInterface);
                        if (faux == null) continue;

                        logger.LogTrace($"Found implementation {type.FullName} for interface {faux.FullName}, marked as registered.");
                        types.Add(faux, type);

                        if (unregistered.Contains(faux))
                        {
                            logger.LogTrace($"Interface {faux.FullName} was marked as unregistered, removing from that list.");
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
                    if (!references.ContainsKey(dependency.FullName))
                    {
                        DiscoverMappings(Assembly.Load(dependency), types);
                    }
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