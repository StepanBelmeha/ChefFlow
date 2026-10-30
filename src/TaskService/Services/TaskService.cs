using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.Dtos;
using TaskService.Models;

namespace TaskService.Services
{
    public class TaskService(TaskDbContext context, ILogger<TaskService> logger)
    {
        private readonly TaskDbContext _context = context;
        private readonly ILogger<TaskService> _logger = logger;

        public async Task<List<TaskDto>> GetAllAsync(int userId, CancellationToken ct)
        {
            return await _context.Tasks
                .Where(t => t.UserId == userId)
                .Select(t => ToDto(t))
                .ToListAsync(ct);
        }

        public async Task<TaskDto?> GetByIdAsync(int id, CancellationToken ct)
        {
            var task = await _context.Tasks.FindAsync([id], ct);
            return task == null ? null : ToDto(task);
        }

        public async Task<TaskDto> CreateAsync(int userId, CreateTaskDto dto, CancellationToken ct)
        {
            var task = new UserTask
            {
                UserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                Deadline = dto.Deadline,
                Priority = dto.Priority,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Створено завдання {Id} для користувача {UserId}", task.Id, userId);
            return ToDto(task);
        }

        public async Task<TaskDto?> UpdateAsync(int id, int userId, UpdateTaskDto dto, CancellationToken ct)
        {
            var task = await _context.Tasks.FindAsync([id], ct);
            if (task == null) return null;

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Deadline = dto.Deadline;
            task.Priority = dto.Priority;

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Оновлено завдання {Id}", id);
            return ToDto(task);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct)
        {
            var task = await _context.Tasks.FindAsync([id], ct);
            if (task == null) return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Видалено завдання {Id}", id);
            return true;
        }

        public async Task<List<TaskDto>> SearchAsync(int userId, string q, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(q))
                return await GetAllAsync(userId, ct);

            var lower = q.ToLower();
            return await _context.Tasks
                .Where(t => t.UserId == userId &&
                    (t.Title.ToLower().Contains(lower) ||
                     t.Description.ToLower().Contains(lower)))
                .Select(t => ToDto(t))
                .ToListAsync(ct);
        }

        private static TaskDto ToDto(UserTask t) => new()
        {
            Id = t.Id,
            UserId = t.UserId,
            Title = t.Title,
            Description = t.Description,
            Deadline = t.Deadline,
            Priority = t.Priority,
            CreatedAt = t.CreatedAt
        };
    }
}