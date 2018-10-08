using System.Reflection;

namespace DHaven.Faux.Compiler
{
    public interface IFauxFactory
    {
        object Create(TypeInfo type);
    }
}
