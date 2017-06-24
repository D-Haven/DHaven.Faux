#region Copyright 2017 D-Haven.org

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
using System.Reflection;

namespace DHaven.Faux.Compiler
{
    internal class WebServiceComplier
    {
        private readonly TypeInfo typeInfo;

        protected WebServiceComplier(TypeInfo type)
        {
            typeInfo = type;
        }

        public void Verify()
        {
            if (!typeInfo.IsInterface)
            {
                throw new NotSupportedException($"{typeInfo.FullName} must be an interface");
            }
        }

        public object Generate()
        {
            throw new NotImplementedException();
        }
    }

    internal class WebServiceCompiler<TService> : WebServiceComplier
        where TService : class
    {
        public WebServiceCompiler() : base(typeof(TService).GetTypeInfo()) { }

        public new TService Generate()
        {
            return base.Generate() as TService;
        }
    }
}