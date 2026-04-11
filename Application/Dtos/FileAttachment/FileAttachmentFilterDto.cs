namespace Application.Dtos.FileAttachment
{
    public class  FileAttachmentFilterDto
    {
        public string? FilePath { get; set; }
        public int? TodoId { get; set; }
        public long? FileSize { get; set; }
        
    }
}