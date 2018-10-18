using System.IO;
using System.Threading.Tasks;
using DHaven.Faux;

namespace ExampleClient
{
    [FauxClient("blob-service", "api/storage")]
    public interface IBlobService
    {
        [HttpPut("/")]
        [return: ResponseHeader("ETag")]
        Task<string> UploadAsync([Body] Stream content, [RequestHeader("Content-Length")] long length,
            [RequestHeader("Content-Type")] string contentType);

        [HttpGet("/{id}")]
        [return: Body(Format = Format.Raw)]
        Task<Stream> GetAsync([PathValue] string id, [RequestHeader("X-Override-Content-Disposition")] string disposition);
    }
}
