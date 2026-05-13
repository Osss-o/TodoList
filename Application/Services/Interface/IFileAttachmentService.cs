using Application.Dtos.FileAttachment;
using Microsoft.AspNetCore.Http;
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
        Task CreateAsync(FileAttachmentCreateDto dto);
        Task CreateManyAsync(List<IFormFile>files,int todoId);
        Task DeleteAsync(int id);
    }
}
