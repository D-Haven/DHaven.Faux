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
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using TypeInfo = System.Reflection.TypeInfo;

#if NETSTANDARD
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
#endif

namespace DHaven.Faux.Compiler
{
    public partial class WebServiceCompiler
    {
        private enum BindingType { Core, Hystrix }
        private readonly ILogger<WebServiceCompiler> logger;
        private readonly FauxDiscovery fauxDiscovery;
        private readonly IDictionary<BindingType,IWebServiceClassGenerator> serviceClassGenerators = new Dictionary<BindingType,IWebServiceClassGenerator>();

#if NETSTANDARD
        private readonly List<SyntaxTree> syntaxTrees = new List<SyntaxTree>();
#else
        private readonly List<string> codeSources = new List<string>();
#endif

        public WebServiceCompiler(FauxDiscovery fauxDiscovery, IEnumerable<IWebServiceClassGenerator> classGenerators, ILogger<WebServiceCompiler> logger)
        {
            this.logger = logger;
            foreach(var classGenerator in classGenerators)
            {
                var type = classGenerator.GetType().Name.Contains("Hystrix") ? BindingType.Hystrix : BindingType.Core;
                serviceClassGenerators.Add(type, classGenerator);
            }
            this.fauxDiscovery = fauxDiscovery;
            
            foreach(var service in fauxDiscovery.GetAllFauxInterfaces())
            {
                RegisterInterface(service);
            }
        }

        /// <summary>
        /// Registers a new type to be compiled.
        /// </summary>
        /// <param name="type">the type to register</param>
        public void RegisterInterface(TypeInfo type)
        {
            logger.LogDebug($"Registering the interface: {type.FullName}");

            var fullyQualifiedClassName = fauxDiscovery.GetImplementationNameFor(type);
            if (fullyQualifiedClassName != null)
            {
                // already registered
                return;
            }

            var binding = type.GetCustomAttribute<HystrixFauxClientAttribute>() == null ? BindingType.Core : BindingType.Hystrix;
            var sourceCodeList = serviceClassGenerators[binding].GenerateSource(type, out fullyQualifiedClassName);
            fauxDiscovery.RegisterType(type, fullyQualifiedClassName);

            foreach (var sourceCode in sourceCodeList)
            {
#if NETSTANDARD
                syntaxTrees.Add(SyntaxFactory.ParseSyntaxTree(sourceCode));
#else
                codeSources.Add(sourceCode);
#endif
            }

            logger.LogDebug($"Finished compiling the syntax tree for {fullyQualifiedClassName} generated from {type.FullName}");
        }

        public Assembly Compile(string assemblyName)
        {
#if NETSTANDARD
            var somethingToCompile = syntaxTrees.Any();
#else
            var somethingToCompile = codeSources.Any();
#endif

            // Always return something, the entry assembly will be able to load implementations since the assembly
            // is a dependency.  The platform compiled assembly will load what was generated at runtime.
            return somethingToCompile ? PlatformCompile(assemblyName) : Assembly.GetEntryAssembly();
        }

        public string GetImplementationName(TypeInfo type)
        {
            return fauxDiscovery.GetImplementationNameFor(type);
        }
    }

    internal class CompilationException : Exception
    {
        public CompilationException(string message) : base(message)
        {}
    }
}