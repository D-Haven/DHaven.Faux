using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace DHaven.Faux.Compiler
{
    internal class WebServiceCompiler<TService> where TService : class
    {
        private readonly TypeInfo typeInfo = typeof(TService).GetTypeInfo();

        public void Verify()
        {
            if (!typeInfo.IsInterface)
            {
                throw new NotSupportedException($"{typeInfo.FullName} must be an interface");
            }
        }

        public TService Generate()
        {
            throw new NotImplementedException();
        }
    }
}
