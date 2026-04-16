using Microsoft.AspNetCore.Http;

namespace Application.Dtos.FileAttachment
{
    public class FileAttachmentCreateDto
    {
        public int TodoId { get; set; }
        public IFormFile File { get; set; }
    }
}
