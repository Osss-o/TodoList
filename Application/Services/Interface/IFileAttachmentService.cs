using Application.Dtos.FileAttachment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Interface
{
    public interface IFileAttachmentService
    {
        Task<FileAttachmentListDto?> GetByIdAsync(int id);
        Task<List<FileAttachmentListDto>> GetAllAsync(FileAttachmentFilterDto filter);
        Task CreateAsync(FileAttachmentCreateDto fileAttachment);
        Task UpdateAsync(int id,FileAttachmentUpdateDto fileAttachment);
        Task DeleteAsync(int id);
    }
}
