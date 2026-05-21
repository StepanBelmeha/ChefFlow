namespace ChefFlow.API.Controllers
{
    using ChefFlow.API.Data;
    using ChefFlow.API.DTO;
    using ChefFlow.API.Models;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]

    public class UserTasksController(AppDbContext context, ILogger<UserTasksController> logger) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger _logger = logger;

        [HttpGet]
        public IActionResult GetAll([FromQuery] int userId)
        {
            var tasks = _context
                .Tasks
                .Where(t => t.UserId == userId)
                .ToList();
            _logger.LogInformation("Отримано всі завдання для користувача з ID: {UserId}", userId);
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var task = _context.Tasks.Find(id);
            if (task == null)
            {
                _logger.LogWarning("Завдання з ID: {TaskId} не знайдено", id);
                return NotFound();
            }
            _logger.LogInformation("Отримано завдання з ID: {TaskId}", id);
            return Ok(task);
        }

        [HttpPost]
        public IActionResult CreateTask(CreateTaskDTO taskDto)
        {
            var task = new UserTask
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                UserId = taskDto.UserId,
                Priority = taskDto.Priority,
                Deadline = taskDto.Deadline
            };
            _context.Tasks.Add(task);
            _context.SaveChanges();
            _logger.LogInformation("Створено нове завдання: {TaskId}", task.Id);
            return Created($"/api/usertasks/{task.Id}", task);
        }

        [HttpPut]
        [Route("{id}")]
        public IActionResult UpdateTask(int id, UpdateTaskDTO taskDto)
        {
            var task = _context.Tasks.Find(id);
            if (task == null)
            {
                _logger.LogWarning("Спроба оновлення неіснуючого завдання з ID: {TaskId}", id);
                return NotFound();
            }
            task.Title = taskDto.Title;
            task.Description = taskDto.Description;
            task.Deadline = taskDto.Deadline;
            task.Priority = taskDto.Priority;
            _context.SaveChanges();
            _logger.LogInformation("Оновлено завдання з ID: {TaskId}", task.Id);
            return Ok(task);
        }

        [HttpDelete]
        [Route("{id}")]
        public IActionResult DeleteTask(int id)
        {
            var task = _context.Tasks.Find(id);
            if (task == null)
            {
                _logger.LogWarning("Спроба видалення неіснуючого завдання з ID: {TaskId}", id);
                return NotFound();
            }
            _context.Tasks.Remove(task);
            _context.SaveChanges();
            _logger.LogInformation("Видалено завдання з ID: {TaskId}", task.Id);
            return NoContent();
        }

        [HttpGet("search")]
        public IActionResult Search([FromQuery] int userId, [FromQuery] string q)
{
        if (string.IsNullOrWhiteSpace(q))
        {
            var all = _context.Tasks
                .Where(t => t.UserId == userId)
                .ToList();
             return Ok(all);
        }

        var lower = q.ToLower();

        var tasks = _context.Tasks
            .Where(t => t.UserId == userId &&
               (t.Title.ToLower().Contains(lower) ||
                t.Description.ToLower().Contains(lower)))
            .ToList();
        _logger.LogInformation("Пошук завдань для користувача з ID: {UserId} за запитом: {Query}", userId, q);
        return Ok(tasks);
}
    }
}
