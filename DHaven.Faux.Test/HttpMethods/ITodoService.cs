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

using System.Collections.Generic;

namespace DHaven.Faux.Test.HttpMethods
{
    [FauxClient("todo")]
    public interface ITodoService
    {
        [HttpGet]
        [return:Body]
        IEnumerable<KeyValuePair<int,string>> List();

        [HttpGet("{id}")]
        [return:ResponseHeader("Location")]
        string Get([PathValue] int id);

        [HttpPut]
        [return:Body]
        int Add([Body] string todo);

        [HttpDelete("{id}")]
        void Delete([PathValue] int id);
    }
}