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
        Task<FileAttachmentListDto?> GetByIdAsync(int id, int currentUserId, bool isAdmin);
        Task<List<FileAttachmentListDto>> GetAllAsync(FileAttachmentFilterDto filter, int currentUserId, bool isAdmin);
        Task CreateAsync(FileAttachmentCreateDto dto,int currentUserId);
        Task CreateManyAsync(List<IFormFile>files,int todoId,int currentUserId);
        Task UpdateAsync(int id,FileAttachmentUpdateDto fileAttachment, int currentUserId, bool isAdmin);
        Task ReplaceAsync(int oldFileId, IFormFile newFile, int currentUserId);
        Task DeleteAsync(int id, int currentUserId, bool isAdmin);
    }
}
