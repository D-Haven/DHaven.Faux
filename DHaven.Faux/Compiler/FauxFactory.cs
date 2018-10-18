using DHaven.Faux.HttpSupport;
using System.Reflection;

namespace DHaven.Faux.Compiler
{
    public class FauxFactory : IFauxFactory
    {
        private readonly IHttpClient client;
        private readonly WebServiceCompiler compiler;
        private readonly Assembly generatedAssembly;

        public FauxFactory(WebServiceCompiler compiler, IHttpClient client)
        {
            this.client = client;
            this.compiler = compiler;

            generatedAssembly = this.compiler.Compile(null);
        }

        public object Create(TypeInfo type, IHttpClient overrideHttpClient)
        {
            var classname = compiler.GetImplementationName(type);
            var info = generatedAssembly.GetType(classname).GetTypeInfo();
            var constructor = info?.GetConstructor(new[] { typeof(IHttpClient) });
            return constructor?.Invoke(new object[] { overrideHttpClient });
        }

        public object Create(TypeInfo type)
        {
            return Create(type, client);
        }
    }
}
