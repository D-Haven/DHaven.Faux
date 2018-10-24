using DHaven.Faux.HttpSupport;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace DHaven.Faux.Compiler
{
    public class FauxFactory : IFauxFactory
    {
        private readonly IHttpClient client;
        private readonly WebServiceCompiler compiler;
        private readonly Assembly generatedAssembly;
        private readonly ILogger<FauxFactory> logger;

        public FauxFactory(WebServiceCompiler compiler, IHttpClient client, ILogger<FauxFactory> logger)
        {
            this.client = client;
            this.compiler = compiler;
            this.logger = logger;

            generatedAssembly = this.compiler.Compile(null);
        }

        public object Create(TypeInfo type, IHttpClient overrideHttpClient)
        {
            var classname = compiler.GetImplementationName(type);
            var info = generatedAssembly.GetType(classname).GetTypeInfo();
            var constructor = info?.GetConstructor(new[] { typeof(IHttpClient), typeof(ILogger), type });
            // TODO: create the fallback instance here.
            return constructor?.Invoke(new object[] { overrideHttpClient, logger, null});
        }

        public object Create(TypeInfo type)
        {
            return Create(type, client);
        }
    }
}
