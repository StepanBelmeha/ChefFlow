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
        public IActionResult GetAll()
        {
            var tasks = _context
                .Tasks
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
    }
}
