using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.FileAttachment
{
    public class FileAttachmentCreateDto
    {
        public string FilePath { get; set; }
        public int TodoId { get; set; }
        public long FileSize { get; set; }
    }
}
