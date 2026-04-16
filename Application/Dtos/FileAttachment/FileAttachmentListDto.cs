namespace Application.Dtos.FileAttachment
{
    public class FileAttachmentListDto
    {
        public int Id { get; set; }
        public string FilePath { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string TodoTitle { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TodoId { get; set; }
    }
}
