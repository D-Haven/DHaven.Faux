using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DHaven.Faux.Compiler
{
    internal static class AttributeInterpreter
    {
        private static readonly IList<string> ContentHeaders = new List<string>
        {
            "content-type",
            "content-length",
            "content-encoding",
            "content-language",
            "content-location"
        };

        public static void InterpretPathValue(ParameterInfo parameter, StringBuilder classBuilder)
        {
            var pathValue = parameter.GetCustomAttribute<PathValueAttribute>();

            if (pathValue != null)
            {
                var key = string.IsNullOrEmpty(pathValue.Variable) ? parameter.Name : pathValue.Variable;
                classBuilder.AppendLine($"            variables.Add(\"{key}\", {parameter.Name});");
            }
        }

        public static string InterpretRequestHeader(ParameterInfo parameter, StringBuilder classBuilder)
        {
            var requestHeader = parameter.GetCustomAttribute<RequestHeaderAttribute>();

            if (requestHeader != null)
            {
                if (ContentHeaders.Contains(requestHeader.Header.ToLowerInvariant()))
                {
                    return requestHeader.Header;
                }

                classBuilder.AppendLine($"            request.Headers.Add(\"{requestHeader.Header}\", {parameter.Name}{(parameter.ParameterType.IsClass ? "?" : "")}.ToString());");
            }

            return null;
        }
    }
}
