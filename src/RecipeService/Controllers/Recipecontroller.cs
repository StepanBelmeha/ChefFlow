using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeService.DTO;
using RecipeService.Services;

namespace RecipeService.Controllers
{
    [ApiController]
    [Route("api/v1/recipe")]
    public class RecipeController(RecipeService.Services.RecipeService recipeService,
        ILogger<RecipeController> logger) : ControllerBase
    {
        private readonly RecipeService.Services.RecipeService _recipeService = recipeService;

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<RecipeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] bool? isPublished = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool desc = false)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest(new { message = "Некоректні параметри пагінації" });

            var result = await _recipeService.GetAllAsync(
                page, pageSize, search, isPublished, sortBy, desc);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RecipeDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var recipe = await _recipeService.GetByIdAsync(id);
            if (recipe == null) return NotFound();
            return Ok(recipe);
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(RecipeDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateRecipeDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var recipe = await _recipeService.CreateAsync(userId, dto);
            return StatusCode(201, recipe);
        }

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRecipeDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _recipeService.UpdateAsync(id, userId, dto);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPatch("{id}/publish")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Publish(int id)
        {
            var result = await _recipeService.PublishAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _recipeService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}