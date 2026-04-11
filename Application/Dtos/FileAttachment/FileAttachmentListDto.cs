namespace Application.Dtos.FileAttachment
{
    public class FileAttachmentListDto
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string TodoTitle { get; set; }
        public long FileSize { get; set; }
    }
}
