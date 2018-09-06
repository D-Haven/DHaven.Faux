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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DHaven.Faux.HttpSupport;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using TypeInfo = System.Reflection.TypeInfo;

namespace DHaven.Faux.Compiler
{
    public class WebServiceCompiler
    {
        private readonly List<SyntaxTree> syntaxTrees = new List<SyntaxTree>();
        private readonly ISet<string> references = new HashSet<string>();

        private readonly ILogger<WebServiceCompiler> logger =
            DiscoverySupport.LogFactory.CreateLogger<WebServiceCompiler>();
        public static List<string> CodeSources = new List<string>();
        public WebServiceCompiler()
        {
            UpdateReferences(GetType().GetTypeInfo().Assembly);
        }

        public string RegisterInterface(TypeInfo type)
        {
            logger.LogDebug($"Registering the interface: {type.FullName}");
            
            UpdateReferences(type.Assembly);
            var sourceCode = WebServiceClassGenerator.GenerateSource(type, out var fullyQualifiedClassName);
            CodeSources.Add(sourceCode);
            syntaxTrees.Add(SyntaxFactory.ParseSyntaxTree(sourceCode));
            
            logger.LogDebug($"Finished compiling the syntax tree for {fullyQualifiedClassName} generated from {type.FullName}");

            return fullyQualifiedClassName;
        }

        private void UpdateReferences(Assembly assembly)
        {
            var referenceLocation = assembly.Location;

            if (references.Contains(referenceLocation))
            {
                return;
            }

            logger.LogTrace($"Registering reference to {assembly.FullName}");
            references.Add(referenceLocation);

            foreach(var dependency in assembly.GetReferencedAssemblies())
            {
                logger.LogTrace($"Loading dependency {dependency.FullName}");
                UpdateReferences(Assembly.Load(dependency));
            }
        }

        public void Compile(Stream stream, string assemblyName)
        {
            var compilation = CSharpCompilation.Create(assemblyName)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(references.Select(location => MetadataReference.CreateFromFile(location)))
                .AddSyntaxTrees(syntaxTrees);

            var result = compilation.Emit(stream);

            if (result.Success)
            {
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

    internal class CompilationException : Exception
    {
        public CompilationException(string message) : base(message)
        {}
    }
}