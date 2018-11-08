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

using System.IO;
using System.Threading.Tasks;

namespace DHaven.Faux.Test.ParameterTypes
{
    [FauxClient("blob-service", "/api/storage")]
    public interface IBlobStore
    {
        [HttpPost("/")]
        [return: ResponseHeader("ETag")]
        Task<string> Upload([Body(Format = Format.Raw)] Stream content, [RequestHeader("Content-Type")] string mimeType,
            [RequestHeader("Content-Length")] long length, [RequestHeader("Content-Disposition")] string disposition);

        [HttpGet("/{id}")]
        [return: Body(Format = Format.Raw)]
        Task<Stream> Get([PathValue] string id, [RequestParameter("override-content-disposition")]
            string overrideDisposition);

        [HttpDelete("/{id}")]
        [return: Body(Format = Format.Json)]
        Task<bool> Delete([PathValue] string id);

    }
}