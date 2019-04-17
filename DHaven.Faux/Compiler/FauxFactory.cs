using System.Net.Http;
using DHaven.Faux.HttpSupport;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace DHaven.Faux.Compiler
{
    public class FauxFactory : IFauxFactory
    {
        private readonly IHttpClientFactory clientFactory;
        private readonly WebServiceCompiler compiler;
        private readonly Assembly generatedAssembly;
        private readonly ILogger<FauxFactory> logger;

        public FauxFactory(WebServiceCompiler compiler, IHttpClientFactory clientFactory, ILogger<FauxFactory> logger)
        {
            this.clientFactory = clientFactory;
            this.compiler = compiler;
            this.logger = logger;

            generatedAssembly = this.compiler.Compile(null);
        }

        public object Create(TypeInfo type, IHttpClient overrideHttpClient)
        {
            var classname = compiler.GetImplementationName(type);
            var info = generatedAssembly.GetType(classname)?.GetTypeInfo();
            var constructor = info?.GetConstructor(new[] { typeof(IHttpClient), typeof(ILogger) });
            return constructor?.Invoke(new object[] { overrideHttpClient, logger });
        }

        public object Create(TypeInfo type)
        {
            return Create(type, new HttpClientWrapper(clientFactory.CreateClient()));
        }
    }
}
