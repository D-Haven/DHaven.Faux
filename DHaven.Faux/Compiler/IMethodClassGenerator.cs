using System.Reflection;

namespace DHaven.Faux.Compiler
{
    /// <summary>
    /// Used when you need to generate a support class for the method, like HystrixCommands, etc.
    /// </summary>
    public interface IMethodClassGenerator
    {
        CompilerConfig Config { get; }

        string GenerateMethodClass(MethodInfo method, out string fullMethodClassName);
    }
}