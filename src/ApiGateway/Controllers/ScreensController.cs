using ApiGateway.Aggregators;
using ApiGateway.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/v1/screens")]
    [Authorize]
    public class ScreensController(RecipeScreenAggregator aggregator) : ControllerBase
    {
        /// <summary>Екран рецепту: рецепт з автором та мої завдання одним запитом.</summary>
        [HttpGet("recipe/{id}")]
        [ProducesResponseType(typeof(RecipeScreenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> Recipe(int id, CancellationToken ct)
        {
            var token = Request.Headers.Authorization.ToString();
            try
            {
                var screen = await aggregator.BuildAsync(id, token, ct);
                return screen == null ? NotFound() : Ok(screen);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return Problem("RecipeService недоступний", statusCode: 503);
            }
        }
    }
}