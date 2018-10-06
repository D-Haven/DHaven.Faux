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
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using TypeInfo = System.Reflection.TypeInfo;

namespace DHaven.Faux.Compiler
{
    public partial class WebServiceCompiler
    {
        private readonly ISet<string> references = new HashSet<string>();
        private readonly IDictionary<TypeInfo,string> registeredTypes = new Dictionary<TypeInfo,string>();

        private readonly ILogger<WebServiceCompiler> logger =
            FauxConfiguration.LogFactory.CreateLogger<WebServiceCompiler>();

        private readonly IWebServiceClassGenerator serviceClassGenerator;

#if NETSTANDARD
        private readonly List<SyntaxTree> syntaxTrees = new List<SyntaxTree>();
#else
        private readonly List<string> codeSources = new List<string>();
        private readonly HashSet<Assembly> sourceAssemblies = new HashSet<Assembly>();
#endif

        public WebServiceCompiler(IWebServiceClassGenerator classGenerator)
        {
            serviceClassGenerator = classGenerator;
            UpdateReferences(GetType().GetTypeInfo().Assembly);
        }

        /// <summary>
        /// Registers a new type to be compiled.
        /// </summary>
        /// <param name="type">the type to register</param>
        /// <returns>true if this is a new registration or false if it was already registered</returns>
        public bool RegisterInterface(TypeInfo type)
        {
            logger.LogDebug($"Registering the interface: {type.FullName}");

            if (registeredTypes.TryGetValue(type, out var fullyQualifiedClassName))
            {
                return false;
            }
            
            UpdateReferences(type.Assembly);
            var sourceCode = serviceClassGenerator.GenerateSource(type, out fullyQualifiedClassName);
            registeredTypes.Add(type, fullyQualifiedClassName);
            
#if NETSTANDARD
            syntaxTrees.Add(SyntaxFactory.ParseSyntaxTree(sourceCode));
#else
            codeSources.Add(sourceCode);
            sourceAssemblies.Add(type.Assembly);
#endif
            
            logger.LogDebug($"Finished compiling the syntax tree for {fullyQualifiedClassName} generated from {type.FullName}");
            return true;
        }

        public string GetImplementationName(TypeInfo type)
        {
            registeredTypes.TryGetValue(type, out var fullyQualifiedClassName);
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
    }

    internal class CompilationException : Exception
    {
        public CompilationException(string message) : base(message)
        {}
    }
}