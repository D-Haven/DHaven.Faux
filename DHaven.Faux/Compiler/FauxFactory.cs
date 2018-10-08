using DHaven.Faux.HttpSupport;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DHaven.Faux.Compiler
{
    public class FauxFactory : IFauxFactory
    {
        private readonly IHttpClient client;
        private readonly WebServiceCompiler compiler;
        private readonly Assembly generatedAssembly;

        public FauxFactory(IFauxRegistrar registrar, WebServiceCompiler compiler, IHttpClient client)
        {
            this.client = client;
            this.compiler = compiler;

            foreach(var service in registrar.GetRegisteredServices())
            {
                this.compiler.RegisterInterface(service);
            }

            generatedAssembly = this.compiler.Compile(null);
        }

        public object Create(TypeInfo type)
        {
            var classname = compiler.GetImplementationName(type);
            var info = generatedAssembly.GetType(classname).GetTypeInfo();
            var constructor = info?.GetConstructor(new Type[] { typeof(IHttpClient) });
            return constructor?.Invoke(new object[] { client });
        }
    }
}
