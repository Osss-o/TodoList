using Application.Dtos.FileAttachment;
using Application.Repositories.Interface;
using Application.Services.Interface;
using Application.Specifications;
using Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public class FileAttachmentService : IFileAttachmentService
    {
        private readonly IGenericRepository<FileAttachment> _fileRepd;
        private readonly IGenericRepository<Todo> _todoRepo;
        private readonly IWebHostEnvironment _env;
        private readonly ICurrentUserService _currentUserService;

        public FileAttachmentService(
            IGenericRepository<FileAttachment> fileRepd,
            IGenericRepository<Todo> todoRepo,
            IWebHostEnvironment env,
            ICurrentUserService currentUserService)
        {
            _fileRepd = fileRepd;
            _todoRepo = todoRepo;
            _env = env;
            _currentUserService = currentUserService;
        }

        public async Task<FileAttachmentListDto?> GetByIdAsync(int id)
        {
            var currentUserId = _currentUserService.UserId;
            var isAdmin = _currentUserService.IsAdmin;

            var spec = new SpecificationBuilder<FileAttachment>()
               .Where(f => f.Id == id && (isAdmin || f.Todo.UserId == currentUserId))
               .Include(f => f.Todo)
               .Build();

            var file = await _fileRepd.GetEntityWithSpec(spec);

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

        public async Task<List<FileAttachmentListDto>> GetAllAsync(FileAttachmentFilterDto filter)
        {
            var currentUserId = _currentUserService.UserId;
            var isAdmin = _currentUserService.IsAdmin;

            var query = new SpecificationBuilder<FileAttachment>()
                .Include(f => f.Todo);

            if (!isAdmin)
                query.Where(f => f.Todo.UserId == currentUserId);

            if (filter.TodoId.HasValue)
                query.Where(f => f.TodoId == filter.TodoId.Value);

            if (!string.IsNullOrEmpty(filter.FileName))
                query.Where(f => f.FileName.Contains(filter.FileName));

            if (!string.IsNullOrEmpty(filter.ContentType))
                query.Where(f => f.ContentType.Contains(filter.ContentType));

            var spec = query.Build();
            var files = await _fileRepd.ListWithSpecAsync(spec);

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

        public async Task CreateAsync(FileAttachmentCreateDto dto)
        {
            var currentUserId = _currentUserService.UserId;

            await SaveFile(dto.File, dto.TodoId);
        }

        public async Task CreateManyAsync(List<IFormFile> files, int todoId)
        {
            var currentUserId = _currentUserService?.UserId;
            var spec = new SpecificationBuilder<Todo>()
                .Where(t => t.Id == todoId && t.UserId == currentUserId)
                .Build();
            var todo = await _todoRepo.GetEntityWithSpec(spec);

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


     

        public async Task DeleteAsync(int id)
        {
            var currentUserId = _currentUserService.UserId;
            var isAdmin = _currentUserService.IsAdmin;
            var spec = new SpecificationBuilder<FileAttachment>()
                .Where(f => f.Id == id && (isAdmin || f.Todo.UserId == currentUserId))
                .Include(f => f.Todo)
                .Build();

            var file = await _fileRepd.GetEntityWithSpec(spec);

            if (file == null)
                throw new KeyNotFoundException("File not found or access denied.");

            var filePath = Path.Combine(_env.WebRootPath, file.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _fileRepd.Delete(file);
            await _fileRepd.SaveChanges();
        }

        private async Task SaveFile(IFormFile file, int todoId)
        {
            var currentUserId = _currentUserService.UserId;

            var spec = new SpecificationBuilder<Todo>()
                .Where(t => t.Id == todoId && t.UserId == currentUserId)
                .Build();
            var todo = await _todoRepo.GetEntityWithSpec(spec);

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
            var uploadsFolder = Path.Combine(rootPath, "uploads");

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
