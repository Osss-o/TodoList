using Microsoft.AspNetCore.Http;

namespace Application.Dtos.FileAttachment
{
    public class FileAttachmentUpdateDto
    {
        public int? TodoId { get; set; }
        public string? FileName { get; set; }
        public IFormFile? File { get; set; }
        public long? FileSize { get; set; }
        public string? ContentType { get; set; }
    }
}
