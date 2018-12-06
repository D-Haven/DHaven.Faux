using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
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

        public IEnumerable<TypeInfo> GetAllFauxInterfaces()
        {
            return fauxMapping.SpinWaitResult().Keys.Select(t => t.GetTypeInfo())
                .Union(unregistered.Select(t => t.GetTypeInfo()));
        }

        public string GetImplementationNameFor(TypeInfo fauxInterface)
        {
            fauxMapping.SpinWaitResult().TryGetValue(fauxInterface.AsType(), out var implementation);
            var className = implementation?.FullName;

            if (className == null) generatedTypes.TryGetValue(fauxInterface, out className);

            return className;
        }

        public IEnumerable<string> GetReferenceLocations()
        {
            fauxMapping.SpinWaitResult();

            return references.Values.Select(dependency => dependency.Location);
        }

        public IEnumerable<Assembly> GetReferenceAssemblies()
        {
            fauxMapping.SpinWaitResult();

            return references.Values;
        }

        public void RegisterType(TypeInfo fauxInterface, string fullyQualifiedName)
        {
            generatedTypes.Add(fauxInterface, fullyQualifiedName);

            if (!references.ContainsKey(fauxInterface.Assembly.FullName))
            {
                // Just in case, to handle the edge case that the assembly the
                // type is included isn't in the list of references.
                DiscoverMappings(fauxInterface.Assembly, fauxMapping.Result);
            }
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

                        var hystrixAttribute = faux.GetCustomAttribute<HystrixFauxClientAttribute>();
                        if (faux.FullName.Equals(hystrixAttribute?.Fallback?.FullName))
                        {
                            // Ignore Hystrix fallback classes during discovery process.
                            continue;
                        }

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

    internal static class TaskExtensions
    {
        /// <summary>
        /// This is sort of a hack, but the only way to handle the asynchronous task and get the
        /// results on a one core processor is to yield the current thread so that it can finish.
        /// Prevents deadlock on single core VMs like AppVeyor free tier.
        /// </summary>
        /// <typeparam name="T">the return type of the task</typeparam>
        /// <param name="task">the task we are waiting for</param>
        /// <returns>the result of the task</returns>
        internal static T SpinWaitResult<T>(this Task<T> task)
        {
            while (task.Status == TaskStatus.Running)
            {
                Thread.Sleep(1);
            }

            return task.Result;
        }
    }
}