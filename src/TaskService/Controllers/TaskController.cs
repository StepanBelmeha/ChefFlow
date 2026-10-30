using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskService.Dtos;
using TaskService.Services;

namespace TaskService.Controllers
{
    [ApiController]
    [Route("api/v1/tasks")]
    [Authorize]
    public class TasksController(TaskService.Services.TaskService taskService,
        ILogger<TasksController> logger) : ControllerBase
    {
        private readonly TaskService.Services.TaskService _taskService = taskService;

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        [ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var tasks = await _taskService.GetAllAsync(GetUserId(), ct);
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var task = await _taskService.GetByIdAsync(id, ct);
            if (task == null) return NotFound();
            return Ok(task);
        }

        [HttpPost]
        [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateTaskDto dto, CancellationToken ct)
        {
            var task = await _taskService.CreateAsync(GetUserId(), dto, ct);
            return Created($"/api/v1/tasks/{task.Id}", task);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto dto, CancellationToken ct)
        {
            var task = await _taskService.UpdateAsync(id, GetUserId(), dto, ct);
            if (task == null) return NotFound();
            return Ok(task);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _taskService.DeleteAsync(id, ct);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct)
        {
            var tasks = await _taskService.SearchAsync(GetUserId(), q, ct);
            return Ok(tasks);
        }
    }
}