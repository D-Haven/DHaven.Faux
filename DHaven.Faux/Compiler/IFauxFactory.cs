using DHaven.Faux.HttpSupport;
using System.Reflection;

namespace DHaven.Faux.Compiler
{
    public interface IFauxFactory
    {
        void RegisterInterface<TService>() where TService : class;
        object Create(TypeInfo type);
        object Create(TypeInfo type, IHttpClient overrideHttpClient);
    }
}
