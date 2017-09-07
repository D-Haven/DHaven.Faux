#region Copyright 2017 D-Haven.org

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeInfo = System.Reflection.TypeInfo;
using System.Dynamic;
using Steeltoe.Discovery.Client;

namespace DHaven.Faux.Compiler
{
    internal class WebServiceComplier
    {
        private const string RootNamespace = "DHaven.Feign.Wrapper";
        private readonly TypeInfo typeInfo;
        private readonly string newClassName;
        private static Assembly servicesAssembly;
        private static readonly List<SyntaxTree> syntaxTrees = new List<SyntaxTree>();
        private static readonly ISet<string> References = new HashSet<string>
        {
            typeof(DiscoveryAwareBase).GetTypeInfo().Assembly.Location,
            typeof(DynamicObject).GetTypeInfo().Assembly.Location,
            typeof(IDiscoveryClient).GetTypeInfo().Assembly.Location
        };

        protected WebServiceComplier(TypeInfo type)
        {
            typeInfo = type;
            newClassName = $"{RootNamespace}.{typeInfo.Name}";
            UpdateReferences(typeInfo.Assembly);
            Define();
        }

        private void UpdateReferences(Assembly assembly)
        {
            string referenceLocation = assembly.Location;

            if (!References.Contains(referenceLocation))
            {
                References.Add(referenceLocation);

                foreach(var dependency in assembly.GetReferencedAssemblies())
                {
                    UpdateReferences(Assembly.Load(dependency));
                }
            }
        }

        public void Define()
        {
            if (!typeInfo.IsInterface || !typeInfo.IsPublic)
            {
                throw new ArgumentException($"{typeInfo.FullName} must be a public interface");
            }

            if (typeInfo.IsGenericType)
            {
                throw new NotSupportedException($"Generic interfaces are not supported: {typeInfo.FullName}");
            }

            var className = typeInfo.FullName.Replace(".", string.Empty);
            var serviceName = typeInfo.GetCustomAttribute<FauxClientAttribute>().Name;
            var baseRoute = typeInfo.GetCustomAttribute<RouteAttribute>().BaseRoute;

            var classBuilder = new StringBuilder();
            classBuilder.AppendLine($"namespace {RootNamespace}");
            classBuilder.AppendLine("{");
            classBuilder.AppendLine($"    public class {className} : DHaven.Faux.Compiler.DiscoveryAwareBase, {typeInfo.FullName}");
            classBuilder.AppendLine("    {");
            classBuilder.AppendLine($"        public {className}(Steeltoe.Discovery.Client.IDiscoveryClient client)");
            classBuilder.AppendLine($"            : base(client, \"{serviceName}\", \"{baseRoute}\") {{ }}");
            classBuilder.AppendLine("    }");
            classBuilder.AppendLine("}");

            syntaxTrees.Add(SyntaxFactory.ParseSyntaxTree(classBuilder.ToString()));
        }

        public object Generate()
        {
            if (servicesAssembly == null)
            {
                Compile();
                Debug.Assert(servicesAssembly != null);
            }

            var type = servicesAssembly.GetType(newClassName);
            var constructor = type.GetConstructor(new[] { typeof(IDiscoveryClient) });
            return constructor.Invoke(new[] { new DiscoveryClientFactory(new DiscoveryOptions()).CreateClient() });
        }

        private static void Compile()
        {
            var assemblyName = Path.GetRandomFileName();

            var compilation = CSharpCompilation.Create(assemblyName)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(References.Select(location => MetadataReference.CreateFromFile(location)))
                .AddSyntaxTrees(syntaxTrees);

            using (var stream = new MemoryStream())
            {
                var result = compilation.Emit(stream);

                if (result.Success)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    servicesAssembly = AssemblyLoadContext.Default.LoadFromStream(stream);
                    return;
                }

                var failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);

                var errorList = new StringBuilder();
                foreach (var diagnostic in failures)
                {
                    errorList.AppendLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
                }

                throw new CompilationException(errorList.ToString());
            }
        }
    }

    internal class CompilationException : Exception
    {
        public CompilationException(string message) : base(message)
        {}
    }

    internal class WebServiceCompiler<TService> : WebServiceComplier
        where TService : class
    {
        public WebServiceCompiler() : base(typeof(TService).GetTypeInfo()) { }

        public new TService Generate()
        {
            return base.Generate() as TService;
        }
    }
}