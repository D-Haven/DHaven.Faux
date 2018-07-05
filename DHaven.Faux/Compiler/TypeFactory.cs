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

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace DHaven.Faux.Compiler
{
    internal static class TypeFactory
    {
        private static readonly Type[] EmptyTypes = new Type[0];
        private static readonly object[] EmptyParams = new object[0];
        private static Assembly generatedAssembly;

        internal static WebServiceCompiler Compiler { get; } = new WebServiceCompiler();

        internal static TService CreateInstance<TService>(string className)
            where TService : class // really interface
        {
            EnsureAssemblyIsGenerated();
            
            var type = generatedAssembly?.GetType(className);
            var constructor = type?.GetConstructor(EmptyTypes);
            return constructor?.Invoke(EmptyParams) as TService;
        }
                
        private static void EnsureAssemblyIsGenerated()
        {
            // Delay until all services have been registered first.
            if (generatedAssembly == null)
            {
                return;
            }

            using (var stream = new MemoryStream())
            {
                Compiler.Compile(stream);
                stream.Seek(0, SeekOrigin.Begin);
                generatedAssembly = AssemblyLoadContext.Default.LoadFromStream(stream);
            }
                
            Debug.Assert(generatedAssembly != null);
        }
    }
}