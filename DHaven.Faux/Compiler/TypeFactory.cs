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
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace DHaven.Faux.Compiler
{
    internal static class TypeFactory
    {
        private static readonly Type[] EmptyTypes = new Type[0];
        private static readonly object[] EmptyParams = new object[0];
        private static readonly ILogger Logger;
        private static Assembly generatedAssembly;

        static TypeFactory()
        {
            Logger = FauxConfiguration.LogFactory.CreateLogger(typeof(TypeFactory));
        }

        private static WebServiceCompiler Compiler { get; } = new WebServiceCompiler();

        internal static string RegisterInterface<TService>()
        {
            var className = Compiler.RegisterInterface(typeof(TService).GetTypeInfo(), out var alreadyRegistered);

            if (alreadyRegistered || generatedAssembly == null)
            {
                return className;
            }

            Logger.LogWarning("Performance hit, registered a new interface after generating the assembly.  Regenerating now.");

            lock (EmptyTypes)
            {
                // Assembly was already created, so in this case we have to force it to recompile to pick up the
                // new service.
                Interlocked.Exchange(ref generatedAssembly, null);
            }

            return className;
        }

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
            if (generatedAssembly != null)
            {
                return;
            }

            lock (EmptyTypes)
            {
                if(generatedAssembly != null)
                {
                    return;
                }

                Logger.LogInformation("Compiling and loading type assembly in memory.");

                generatedAssembly = Compiler.Compile(null);
            }
            
            Debug.Assert(generatedAssembly != null);
        }
    }
}