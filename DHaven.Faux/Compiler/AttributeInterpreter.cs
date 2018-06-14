using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace DHaven.Faux.Compiler
{
    /// <summary>
    /// Internal helper to generate code to support the special attributes that
    /// are added to the user's interface.
    /// </summary>
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
                classBuilder.AppendLine($"            仮variables.Add(\"{key}\", {parameter.Name});");
            }
        }

        public static void InterpretRequestHeader(ParameterInfo parameter, Dictionary<string, ParameterInfo> requestHeaders, Dictionary<string, ParameterInfo> contentHeaders)
        {
            var requestHeader = parameter.GetCustomAttribute<RequestHeaderAttribute>();

            if (requestHeader == null)
            {
                return;
            }

            if (ContentHeaders.Contains(requestHeader.Header.ToLowerInvariant()))
            {
                contentHeaders.Add(requestHeader.Header, parameter);
            }
            else
            {
                requestHeaders.Add(requestHeader.Header, parameter);
            }
        }

        public static void InterpretBodyParameter(ParameterInfo parameter, ref ParameterInfo bodyParam, ref BodyAttribute bodyAttr)
        {
            var attr = parameter.GetCustomAttribute<BodyAttribute>();

            if (attr == null)
            {
                return;
            }

            if (bodyAttr != null)
            {
                throw new WebServiceCompileException("Cannot have more than one body parameter");
            }

            bodyAttr = attr;
            bodyParam = parameter;
        }

        public static bool CreateContentObjectIfSpecified(BodyAttribute bodyAttr, ParameterInfo bodyParam, StringBuilder clasStringBuilder)
        {
            if (bodyAttr == null || bodyParam == null)
            {
                return false;
            }

            var format = bodyAttr.Format;

            if (format == Format.Auto)
            {
                format = typeof(Stream).IsAssignableFrom(bodyParam.ParameterType) ? Format.Raw : Format.Json;
            }

            switch (format)
            {
                case Format.Json:
                    clasStringBuilder.AppendLine($"            var 仮content = ConvertToJson({bodyParam.Name});");
                    break;
                case Format.Raw:
                    clasStringBuilder.AppendLine($"            var 仮content = StreamRawContent({bodyParam.Name});");
                    break;
                default:
                    return false;
            }

            return true;
        }

        public static void InterpretRequestParameter(ParameterInfo parameter, StringBuilder classBuilder)
        {
            var paramAttribute = parameter.GetCustomAttribute<RequestParameterAttribute>();

            if (paramAttribute == null)
            {
                return;
            }

            var paramName = string.IsNullOrEmpty(paramAttribute.Parameter)
                ? parameter.Name
                : paramAttribute.Parameter;

            classBuilder.AppendLine($"            仮reqParams.Add(\"{paramName}\", {parameter.Name}{(parameter.ParameterType.IsClass ? "?" : "")}.ToString());");
        }

        public static void InterpretResponseHeaderInParameters(ParameterInfo parameter, bool isAsync, ref Dictionary<string,ParameterInfo> responseHeaders)
        {
            var responseAttribute = parameter.GetCustomAttribute<ResponseHeaderAttribute>();

            if (responseAttribute == null)
            {
                return;
            }

            if (!parameter.IsOut)
            {
                throw new WebServiceCompileException("[ResponseHeaderAttribute] must be used on out paramters or for the return type.");
            }

            if (isAsync)
            {
                throw new WebServiceCompileException(
                    "[ResponseHeaderAttribute] in the parameter list cannot be used with async service calls.");
            }

            responseHeaders.Add(responseAttribute.Header, parameter);
        }
    }
}
