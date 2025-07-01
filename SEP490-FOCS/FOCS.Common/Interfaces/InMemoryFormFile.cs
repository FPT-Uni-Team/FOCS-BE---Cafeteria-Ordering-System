using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public class InMemoryFormFile : IFormFile
    {
        private readonly Stream _stream;

        public InMemoryFormFile(byte[] fileBytes, string fileName, string contentType)
        {
            _stream = new MemoryStream(fileBytes);
            FileName = fileName;
            ContentType = contentType;
            Length = fileBytes.Length;
            Name = "qrCode";
        }

        public string ContentType { get; }
        public string ContentDisposition => $"form-data; name=\"{Name}\"; filename=\"{FileName}\"";
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public long Length { get; }
        public string Name { get; }
        public string FileName { get; }

        public void CopyTo(Stream target) => _stream.CopyTo(target);
        public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default) => _stream.CopyToAsync(target, cancellationToken);
        public Stream OpenReadStream() => _stream;
    }
}
