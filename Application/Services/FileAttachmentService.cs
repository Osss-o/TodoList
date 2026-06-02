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
        private readonly IGenericRepository<FileAttachment> _fileRepo;
        private readonly IGenericRepository<Todo> _todoRepo;
        private readonly IWebHostEnvironment _env;
        private readonly ICurrentUserService _currentUserService;

        public FileAttachmentService(
            IGenericRepository<FileAttachment> fileRepo,
            IGenericRepository<Todo> todoRepo,
            IWebHostEnvironment env,
            ICurrentUserService currentUserService)
        {
            _fileRepo = fileRepo;
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

            var file = await _fileRepo.GetEntityWithSpec(spec);

            if (file == null) return null;

            return new FileAttachmentListDto
            {
                Id = file.Id,
                TodoId = file.TodoId,
                TodoTitle = file.Todo?.Title,
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
            var files = await _fileRepo.ListWithSpecAsync(spec);

            return files.Select(f => new FileAttachmentListDto
            {
                Id = f.Id,
                TodoId = f.TodoId,
                TodoTitle = f.Todo?.Title,
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
            var isAdmin = _currentUserService.IsAdmin;

            var spec = new SpecificationBuilder<Todo>()
                    .Where(t => t.Id == dto.TodoId && (isAdmin || t.UserId == currentUserId))
                    .Build();
            var todoExists = await _todoRepo.AnyAsync(spec);

            if (!todoExists)
                throw new KeyNotFoundException($"Todo with ID {dto.TodoId} not found or access denied.");

            var attachment = await SaveFileInternalAsync(dto.File, dto.TodoId);

            await _fileRepo.Insert(attachment);
            await _fileRepo.SaveChanges();
        }

        public async Task CreateManyAsync(List<IFormFile> files, int todoId)
        {
            var currentUserId = _currentUserService?.UserId;
            var isAdmin = _currentUserService?.IsAdmin ?? false;

            var spec = new SpecificationBuilder<Todo>()
                .Where(t => t.Id == todoId && (isAdmin || t.UserId == currentUserId))
                .Build();
            var todo = await _todoRepo.AnyAsync(spec);

            if (!todo)
                throw new KeyNotFoundException($"Todo with ID {todoId} not found or access denied.");

            var attachments = new List<FileAttachment>();

            foreach (var file in files)
            {
                if (file != null && file.Length > 0)
                {
                    var attachment = await SaveFileInternalAsync(file, todoId);
                    attachments.Add(attachment);
                }
            }

            if (attachments.Any())
            {
                await _fileRepo.InsertRange(attachments);
                await _fileRepo.SaveChanges();
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

            var file = await _fileRepo.GetEntityWithSpec(spec);

            if (file == null)
                throw new KeyNotFoundException("File not found or access denied.");

            var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var filePath = Path.Combine(rootPath, file.FilePath.TrimStart('/', '\\'));

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            await _fileRepo.Delete(file);
            await _fileRepo.SaveChanges();
        }


        private async Task<FileAttachment> SaveFileInternalAsync(IFormFile file, int todoId)
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
                TodoId = todoId,
            };
        }
    }
}
