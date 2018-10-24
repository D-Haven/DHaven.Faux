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

using System.Threading.Tasks;

namespace DHaven.Faux.Test.ReturnTypes
{
    public class Value
    {
        public string Content { get; set; }
        public bool IsValid { get; set; }
    }
    
    //TODO: reanable [FauxClient("return")]
    public interface IReturnService
    {
        [HttpPut]
        [return:ResponseHeader("Location")]
        Task<string> PutAsync();

        [HttpPut]
        [return:ResponseHeader("Location")]
        string Put();

        [HttpPost("echo")]
        [return: Body(Format = Format.Json)]
        Task<Value> EchoAsync([Body] Value val);

        [HttpPost("echo")]
        [return: Body(Format = Format.Json)]
        Value Echo([Body] Value val);
        
        [HttpGet("{id}")]
        [return:Body]
        Value Get([PathValue] int id,
            [ResponseHeader("Location")] out string location,
            [ResponseHeader("Content-Type")] out string mimeType);
    }
}