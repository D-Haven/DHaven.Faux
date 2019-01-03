using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Steeltoe.CircuitBreaker.Hystrix;

namespace DHaven.Faux.Compiler
{
    internal static class CompilerUtils
    {
        /// <summary>
        /// This is the smallest footprint to ensure the Hystrix support is included
        /// without actually affecting the coverage report too much.  A code reference
        /// to a type in that library is all that is required.  The value is null and
        /// the field is not used.
        /// </summary>
#pragma warning disable 169
        private static readonly IHystrixCommandGroupKey IgnoreMe;
#pragma warning restore 169
        
        public static string ToParameterDeclaration(ParameterInfo parameter)
        {
            return $"{ToCompilableName(parameter.ParameterType, parameter.IsOut)} {parameter.Name}";
        }

        public static string ToParameterUsage(ParameterInfo parameter)
        {
            return parameter.IsOut ? $"out {parameter.Name}" : $"{parameter.Name}";
        }

        public static string ToCompilableName(HttpMethod method)
        {
            var value = method.Method.Substring(0, 1) + method.Method.Substring(1).ToLower();
            return $"System.Net.Http.HttpMethod.{value}";
        }

        private static string ToCompilableName(Type type, bool isOut)
        {
            var name = ToCompilableName(type);

            return isOut ? $"out {name}" : name;
        }

        public static string ToCompilableName(Type type)
        {
            var baseName = type.FullName;
            Debug.Assert(baseName != null, nameof(baseName) + " != null");
            
            // If we have a ref or an out parameter, then Type.Name appends '&' to the end.
            if (baseName.EndsWith("&"))
            {
                baseName = baseName.Substring(0, baseName.Length - 1);
            }

            if (!type.IsConstructedGenericType)
            {
                return baseName;
            }

            baseName = baseName.Substring(0, baseName.IndexOf('`'));
            return $"{baseName}<{string.Join(",", type.GetGenericArguments().Select(ToCompilableName))}>";
        }
    }
}