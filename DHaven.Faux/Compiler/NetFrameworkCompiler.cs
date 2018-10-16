using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace DHaven.Faux.Compiler
{
    partial class WebServiceCompiler
    {
#if !NETSTANDARD
        private Assembly PlatformCompile(string assemblyName)
        {
            CompilerParameters compilerParameters = new CompilerParameters();

            var assemblies = new HashSet<string>()
            {
                "System.Net.Http.dll",
            };

            foreach (var assembly in fauxDiscovery.GetReferenceAssemblies().Result)
            {
                var assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(assembly.CodeBase));
                assemblies.Add(assemblyPath);
            }
            assemblies.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DHaven.Faux.dll"));

            compilerParameters.ReferencedAssemblies.AddRange(assemblies.ToArray());
            compilerParameters.GenerateExecutable = false;

            if (string.IsNullOrEmpty(assemblyName))
            {
                compilerParameters.GenerateInMemory = true;
            }
            else
            {
                compilerParameters.OutputAssembly = assemblyName;
            }

            var domProvider = CodeDomProvider.CreateProvider("CSharp");
            var result = domProvider.CompileAssemblyFromSource(compilerParameters, codeSources.ToArray());

            if (!result.Errors.HasErrors)
            {
                return result.CompiledAssembly;
            }

            var failures = result.Errors.OfType<CompilerError>().Where(error => !error.IsWarning).ToList();

            var errorList = new StringBuilder();
            foreach (CompilerError error in failures)
            {
                errorList.AppendLine($"{error.ErrorNumber}:{error.ErrorText} in {error.FileName} Line {error.Line} Col {error.Column}");
            }

            throw new CompilationException(errorList.ToString());
        }
#endif
    }
}
