using Application.Dtos.FileAttachment;
using Application.Repositories.Interface;
using Application.Services.Interface;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class FileAttachmentService : IFileAttachmentService
    {
        private readonly IGenericRepository<FileAttachment> _fileRepd;
        private readonly IGenericRepository<Todo> _todoRepo;
        private readonly IWebHostEnvironment _env;
        private readonly DbContext _dbContext;

        public FileAttachmentService(
            IGenericRepository<FileAttachment> fileRepd,
            IGenericRepository<Todo> todoRepo,
            IWebHostEnvironment env,
            DbContext dbContext)
        {
            _fileRepd = fileRepd;
            _todoRepo = todoRepo;
            _env = env;
            _dbContext = dbContext;
        }

        public async Task<FileAttachmentListDto?> GetByIdAsync(int id, int currentUserId, bool isAdmin)
        {
            var query = _fileRepd.GetAll().Include(f => f.Todo);

            var file = isAdmin
                ? await query.FirstOrDefaultAsync(f => f.Id == id)
                : await query.FirstOrDefaultAsync(f => f.Id == id && f.Todo.UserId == currentUserId);

            if (file == null) return null;

            return new FileAttachmentListDto
            {
                Id = file.Id,
                TodoId = file.TodoId,
                TodoTitle = file.Todo.Title,
                FileName = file.FileName,
                FilePath = file.FilePath,
                ContentType = file.ContentType,
                FileSize = file.FileSize,
                CreatedAt = file.CreatedAt
            };
        }

        public async Task<List<FileAttachmentListDto>> GetAllAsync(FileAttachmentFilterDto filter, int currentUserId, bool isAdmin)
        {
           IQueryable<FileAttachment> query = _fileRepd.GetAll().Include(f => f.Todo);

            if (!isAdmin)
                query = query.Where(f => f.Todo.UserId == currentUserId);

            if (filter.TodoId.HasValue)
                query = query.Where(f => f.TodoId == filter.TodoId.Value);

            if (!string.IsNullOrEmpty(filter.FileName))
                query = query.Where(f => f.FileName.Contains(filter.FileName));

            if (!string.IsNullOrEmpty(filter.ContentType))
                query = query.Where(f => f.ContentType.Contains(filter.ContentType));

            var files = await query.ToListAsync();

            return files.Select(f => new FileAttachmentListDto
            {
                Id = f.Id,
                TodoId = f.TodoId,
                TodoTitle = f.Todo.Title,
                FileName = f.FileName,
                FilePath = f.FilePath,
                ContentType = f.ContentType,
                FileSize = f.FileSize,
                CreatedAt = f.CreatedAt
            }).ToList();
        }

        public async Task CreateAsync(FileAttachmentCreateDto dto, int currentUserId)
        {
            await SaveFile(dto.File, dto.TodoId, currentUserId);
        }

        public async Task CreateManyAsync(List<IFormFile> files, int todoId, int currentUserId)
        {
            var todo = await _todoRepo.GetAll()
                .FirstOrDefaultAsync(t => t.Id == todoId && t.UserId == currentUserId);

            if (todo == null)
                throw new KeyNotFoundException($"Todo with ID {todoId} not found or access denied.");

            var attachments = new List<FileAttachment>();

            foreach (var file in files)
            {
                if (file != null && file.Length > 0)
                {
                    var attachment = await SaveFileInternal(file, todo);
                    attachments.Add(attachment);
                }
            }

            if (attachments.Any())
            {
                await _fileRepd.InsertRange(attachments);
                await _fileRepd.SaveChanges();
            }
        }

        public async Task UpdateAsync(int id, FileAttachmentUpdateDto fileAttachment, int currentUserId, bool isAdmin)
        {
            var query = _fileRepd.GetAll().Include(f => f.Todo);

            var file = isAdmin
                ? await query.FirstOrDefaultAsync(f => f.Id == id)
                : await query.FirstOrDefaultAsync(f => f.Id == id && f.Todo.UserId == currentUserId);

            if (file == null)
                throw new KeyNotFoundException("File not found or access denied.");

            file.FileName = fileAttachment.FileName;
            if (!string.IsNullOrEmpty(fileAttachment.ContentType))
                file.ContentType = fileAttachment.ContentType;
            if (fileAttachment.FileSize.HasValue)
                file.FileSize = fileAttachment.FileSize.Value;

            _fileRepd.Update(file);
            await _fileRepd.SaveChanges();
        }

        public async Task ReplaceAsync(int oldFileId, IFormFile newFile, int currentUserId)
        {
            var oldFile = await _fileRepd.GetAll()
                .Include(f => f.Todo)
                .FirstOrDefaultAsync(f => f.Id == oldFileId && f.Todo.UserId == currentUserId);

            if (oldFile == null)
                throw new KeyNotFoundException("Original file not found or access denied.");

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var oldPath = Path.Combine(_env.WebRootPath, oldFile.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);

                _fileRepd.Delete(oldFile);
                await _fileRepd.SaveChanges();

                await SaveFile(newFile, oldFile.TodoId, currentUserId);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteAsync(int id, int currentUserId, bool isAdmin)
        {
            var query = _fileRepd.GetAll().Include(f => f.Todo);

            var file = isAdmin
                ? await query.FirstOrDefaultAsync(f => f.Id == id)
                : await query.FirstOrDefaultAsync(f => f.Id == id && f.Todo.UserId == currentUserId);

            if (file == null)
                throw new KeyNotFoundException("File not found or access denied.");

            var filePath = Path.Combine(_env.WebRootPath, file.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _fileRepd.Delete(file);
            await _fileRepd.SaveChanges();
        }

        private async Task SaveFile(IFormFile file, int todoId, int currentUserId)
        {
            var todo = await _todoRepo.GetAll()
                .FirstOrDefaultAsync(t => t.Id == todoId && t.UserId == currentUserId);

            if (todo == null)
                throw new KeyNotFoundException($"Todo with ID {todoId} not found or access denied.");

            var attachment = await SaveFileInternal(file, todo);

            await _fileRepd.Insert(attachment);
            await _fileRepd.SaveChanges();
        }

        private async Task<FileAttachment> SaveFileInternal(IFormFile file, Todo todo)
        {
            const long maxFileSize = 2 * 1024 * 1024; // 2 MB

            if (file == null || file.Length == 0)
                throw new InvalidOperationException("No file uploaded.");

            if (file.Length > maxFileSize)
                throw new InvalidOperationException("File size exceeds the maximum allowed limit of 2 MB.");
          
            var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
          
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return new FileAttachment
            {
                FileName = file.FileName,
                FilePath = "uploads/" + uniqueFileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                CreatedAt = DateTime.UtcNow,
                TodoId = todo.Id,
               
            };
        }
    }
}
