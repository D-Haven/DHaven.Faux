using System.Reflection;

namespace DHaven.Faux.Compiler
{
    public interface IStarterAssembly
    {
        Assembly Assembly { get; }
    }

    class StarterAssembly : IStarterAssembly
    {
        public StarterAssembly(Assembly starter)
        {
            Assembly = starter ?? Assembly.GetEntryAssembly();
        }
        
        public Assembly Assembly { get; }
    }
}