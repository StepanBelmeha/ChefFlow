namespace ChefFlow.API.Controllers
{
    using ChefFlow.API.Data;
    using ChefFlow.API.DTO;
    using ChefFlow.API.Models;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]

    public class UserTasksController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        [HttpGet]
        public IActionResult GetAll([FromQuery] int userId)
        {
            var tasks = _context
                .Tasks
                .Where(t => t.UserId == userId)
                .ToList();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var task = _context.Tasks.Find(id);
            if (task == null)
            {
                return NotFound();
            }
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
            return Ok(task);
        }

        [HttpPut]
        [Route("{id}")]
        public IActionResult UpdateTask(int id, UpdateTaskDTO taskDto)
        {
            var task = _context.Tasks.Find(id);
            if (task == null)
            {
                return NotFound();
            }
            task.Title = taskDto.Title;
            task.Description = taskDto.Description;
            task.Deadline = taskDto.Deadline;
            task.Priority = taskDto.Priority;
            _context.SaveChanges();
            return Ok(task);
        }

        [HttpDelete]
        [Route("{id}")]
        public IActionResult DeleteTask(int id)
        {
            var task = _context.Tasks.Find(id);
            if (task == null)
            {
                return NotFound();
            }
            _context.Tasks.Remove(task);
            _context.SaveChanges();
            return Ok();
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

        return Ok(tasks);
}
    }
}
