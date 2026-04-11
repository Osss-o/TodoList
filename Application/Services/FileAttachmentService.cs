using Application.Dtos.FileAttachment;
using Application.Repositories.Interface;
using Application.Services.Interface;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class FileAttachmentService : IFileAttachmentService
    {
        private readonly IGenericRepository<FileAttachment> _fileRepd;
        private readonly IGenericRepository<Todo> _todoRepo;

        public FileAttachmentService(
            IGenericRepository<FileAttachment> fileRepd
            , IGenericRepository<Todo> todoRepo)
        {
            _fileRepd = fileRepd;
            _todoRepo = todoRepo;
        }

        public async Task CreateAsync(FileAttachmentCreateDto fileAttachment)
        {
            const long maxFileSize = 2 * 1024 * 1024; // 2 MB

            if (fileAttachment.FileSize > maxFileSize)
            {
                throw new InvalidOperationException("File size exceeds the maximum allowed limit of 2 MB.");
            }
            var fileAttachmentEntity = new FileAttachment
            {
                FilePath = fileAttachment.FilePath.Trim(),
                FileSize = fileAttachment.FileSize,
                TodoId = fileAttachment.TodoId
            };
            await _fileRepd.Insert(fileAttachmentEntity);
            await _fileRepd.SaveChanges();
        }

        public async Task DeleteAsync(int id)
        {
            var file = await _fileRepd.GetById(id);

            if (file == null)
            {
                throw new KeyNotFoundException($"File attachment with ID {id} not found.");
            }
            if (System.IO.File.Exists(file.FilePath))
            {
                System.IO.File.Delete(file.FilePath);
            }
            _fileRepd.Delete(file);
            await _fileRepd.SaveChanges();
        }

        public async Task<List<FileAttachmentListDto>> GetAllAsync(FileAttachmentFilterDto filter)
        {
            IQueryable<FileAttachment> query = _fileRepd.GetAll()
                .Include(f => f.Todo);

            if (!string.IsNullOrWhiteSpace(filter.FilePath))
            {
                var normalizadPath = filter.FilePath.Trim().ToLower();
                query = query.Where(f => f.FilePath.ToLower().Contains(normalizadPath));
            }
            if (filter.TodoId.HasValue)
            {
                query = query.Where(f => f.TodoId == filter.TodoId.Value);
            }
            if (filter.FileSize.HasValue)
            {
                query = query.Where(f => f.FileSize == filter.FileSize.Value);
            }
            var files = await query.ToListAsync();
            return files.Select(file => new FileAttachmentListDto
            {
                Id = file.Id,
                FilePath = file.FilePath,
                FileSize = file.FileSize,
                TodoTitle = file.Todo?.Title ?? "N/A"
            }).ToList();
        }

        public async Task<FileAttachmentListDto?> GetByIdAsync(int id)
        {
            var file = await _fileRepd.GetAll()
                 .Include(f => f.Todo)
                 .FirstOrDefaultAsync(f => f.Id == id);
            if (file == null)
            {
                return null;
            }
            return new FileAttachmentListDto
            {
                Id = file.Id,
                FilePath = file.FilePath,
                FileSize = file.FileSize,
                TodoTitle = file.Todo?.Title ?? "N/A"
            };
        }

        public async Task UpdateAsync(int id, FileAttachmentUpdateDto fileAttachment)
        {
            var file = await _fileRepd.GetById(id);
            if (file == null)
            {
                throw new KeyNotFoundException($"File attachment with ID {id} not found.");
            }
            if (!string.IsNullOrWhiteSpace(fileAttachment.FilePath))
            {
                file.FilePath = fileAttachment.FilePath.Trim();
            }
            if (fileAttachment.FileSize.HasValue)
            {
                if (fileAttachment.FileSize.Value > 2 * 1024 * 1024)
                {
                    throw new InvalidOperationException("File size exceeds the maximum allowed limit of 2 MB.");
                }
                file.FileSize = fileAttachment.FileSize.Value;
            }
            if (fileAttachment.TodoId.HasValue)
            {
                var todo = await _todoRepo.GetById(fileAttachment.TodoId.Value);
                if (file == null)
                {
                    throw new KeyNotFoundException($"Todo with ID {fileAttachment.TodoId.Value} not found.");
                }
                else
                {
                    file.TodoId = todo.Id;
                }
                _fileRepd.Update(file);
                await _fileRepd.SaveChanges();
            }
        }
    }
}
