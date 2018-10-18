using System.Text;
using System.Linq;
using System.IO;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;

namespace DHaven.Faux.Compiler
{
    partial class WebServiceCompiler
    {
#if NETSTANDARD
        private Assembly PlatformCompile(string assemblyName)
        {
            using (var stream = string.IsNullOrEmpty(assemblyName) 
                ? (Stream)new MemoryStream() 
                : new FileStream(assemblyName, FileMode.Create))
            {
                var compilation = CSharpCompilation.Create(assemblyName ?? Path.GetRandomFileName())
                    .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                    .AddReferences(fauxDiscovery.GetReferenceLocations().Select(location => MetadataReference.CreateFromFile(location)))
                    .AddSyntaxTrees(syntaxTrees);

                var result = compilation.Emit(stream);

                if (result.Success)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    return AssemblyLoadContext.Default.LoadFromStream(stream);
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
#endif
    }
}
