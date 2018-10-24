using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;

static internal class CompilerUtils
{
    public static string ToCompilableName(HttpMethod method)
    {
        var value = method.Method.Substring(0, 1) + method.Method.Substring(1).ToLower();
        return $"System.Net.Http.HttpMethod.{value}";
    }

    public static string ToCompilableName(Type type, bool isOut)
    {
        var name = ToCompilableName(type);

        return !isOut ? name : $"out {name}";
    }

    internal static string ToCompilableName(Type type)
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
        return $"{baseName}<{String.Join(",", type.GetGenericArguments().Select(ToCompilableName))}>";
    }
}