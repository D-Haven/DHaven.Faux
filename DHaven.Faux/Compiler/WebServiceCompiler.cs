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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TypeInfo = System.Reflection.TypeInfo;
using System.Text.RegularExpressions;

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
            typeof(string).GetTypeInfo().Assembly.Location
        };

        protected WebServiceComplier(TypeInfo type)
        {
            typeInfo = type;
            newClassName = $"{RootNamespace}.{typeInfo.Name}";
            References.Add(typeInfo.Assembly.Location);
            Define();
        }

        public void Define()
        {
            if (!typeInfo.IsInterface)
            {
                throw new ArgumentException($"{typeInfo.FullName} must be an interface");
            }

            var classDeclaration = SyntaxFactory.ClassDeclaration(typeInfo.Name);

            classDeclaration.AddMembers(typeInfo.GetMethods().Select(CreateWrapper).ToArray());

            var compilationUnit = SyntaxFactory.CompilationUnit();
            compilationUnit.AddMembers(
                SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(RootNamespace))
                .AddMembers(classDeclaration)
            );

            syntaxTrees.Add(compilationUnit.SyntaxTree);
        }

        private MemberDeclarationSyntax CreateWrapper(MethodInfo method)
        {
            HttpMethodAttribute callType = method.GetCustomAttribute<HttpMethodAttribute>();

            if (callType == null)
            {
                throw new ArgumentException($"{method.Name}() is not a web service call");
            }

            string endpoint = callType.Path ?? string.Empty;
            object data;

            foreach (ParameterInfo param in method.GetParameters())
            {
                BodyAttribute body = param.GetCustomAttribute<BodyAttribute>();
                if (body != null)
                {
                    data = body;
                    continue;
                }

                PathValueAttribute path = param.GetCustomAttribute<PathValueAttribute>();
                if (path != null)
                {
                    // path replacement
                    string pathVariable = string.IsNullOrEmpty(path.Variable) ? param.Name : path.Variable;
                    throw new NotSupportedException("Path Variables are not supported yte.");
                    continue;
                }

                RequestHeaderAttribute header = param.GetCustomAttribute<RequestHeaderAttribute>();
                // request header
                if(header != null)
                {
                    throw new NotSupportedException("Request Headers are not supported yet.");
                    continue;
                }

                RequestParameterAttribute reqParam = param.GetCustomAttribute<RequestParameterAttribute>();
                // request parameter
                if(reqParam != null)
                {
                    string paramName = reqParam.Parameter ?? param.Name;
                    throw new NotSupportedException("Request Parameters are not supported yet.");
                    continue;
                }
            }

            throw new NotImplementedException("We haven't generated the method yet.  Sorry.");
        }

        public object Generate()
        {
            if (servicesAssembly == null)
            {
                Compile();
                Debug.Assert(servicesAssembly != null);
            }

            var type = servicesAssembly.GetType(newClassName);
            var constructor = type.GetConstructor(new Type[0]);
            return constructor.Invoke(new object[0]);
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