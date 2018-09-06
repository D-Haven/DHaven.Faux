#region Copyright 2018 D-Haven.org
// 
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

using DHaven.Faux.HttpSupport;
using Microsoft.Extensions.Logging;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CSharp;

namespace DHaven.Faux.Compiler
{
    internal static class TypeFactory
    {
        private static readonly Type[] EmptyTypes = new Type[0];
        private static readonly object[] EmptyParams = new object[0];
        private static readonly ILogger Logger;
        private static Assembly generatedAssembly;
        private static HashSet<string> assemblies = new HashSet<string>()
        {
            "System.Net.Http.dll"
        };
        static TypeFactory()
        {
            Logger = DiscoverySupport.LogFactory.CreateLogger(typeof(TypeFactory));
        }

        internal static WebServiceCompiler Compiler { get; } = new WebServiceCompiler();

        internal static TService CreateInstance<TService>(string className)
            where TService : class // really interface
        {
            var typeInfo = typeof(TService);
            EnsureAssemblyIsGenerated(typeInfo);

            var type = generatedAssembly?.GetType(className);
            var constructor = type?.GetConstructor(EmptyTypes);
            return constructor?.Invoke(EmptyParams) as TService;
        }

        private static void EnsureAssemblyIsGenerated(Type teType)
        {
            lock (EmptyTypes)
            {
                Logger.LogInformation("Compiling and loading type assembly in memory.");

#if !NETSTANDARD
                CSharpCodeProvider objCSharpCodeProvider = new CSharpCodeProvider();
                ICodeCompiler objICodeCompiler = objCSharpCodeProvider.CreateCompiler();
                CompilerParameters objCompilerParameters = new CompilerParameters();
                var entryAssembly=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,Path.GetFileName(teType.Assembly.CodeBase));
                assemblies.Add(entryAssembly);
                assemblies.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DHaven.Faux.dll"));
                objCompilerParameters.ReferencedAssemblies.AddRange(assemblies.ToArray());
                objCompilerParameters.GenerateExecutable = false;
                objCompilerParameters.GenerateInMemory = true;
                CompilerResults cr = objICodeCompiler.CompileAssemblyFromSourceBatch(objCompilerParameters, WebServiceCompiler.CodeSources.ToArray());
                generatedAssembly = cr.CompiledAssembly;
#endif
#if NETSTANDARD
                using (var stream = new MemoryStream())
                {
                    Compiler.Compile(stream, Path.GetRandomFileName());
                    stream.Seek(0, SeekOrigin.Begin);
                    generatedAssembly = AssemblyLoadContext.Default.LoadFromStream(stream);
                }
#endif
            }
            Debug.Assert(generatedAssembly != null);
        }
    }
}