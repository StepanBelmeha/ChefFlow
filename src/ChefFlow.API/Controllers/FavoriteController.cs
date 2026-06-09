namespace ChefFlow.API.Controllers
{
    using ChefFlow.API.Data;
    using ChefFlow.API.DTO;
    using ChefFlow.API.Models;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class FavoriteController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        [HttpGet("{userId}")]
        public IActionResult GetByuserId(int userId)
        {
            var favorites = _context.Favorites
                .Where(f => f.UserId == userId)
                .Join(_context.Recipes,
                    f => f.RecipeId,
                    r => r.Id,
                    (f, r) => r)
                .Join(_context.Users,
                    r => r.UserId,
                    u => u.Id,
                    (r, u) => new RecipeResponseDTO
                    {
                        Id = r.Id,
                        UserId = r.UserId,
                        Title = r.Title,
                        Description = r.Description,
                        Instructions = r.Instructions,
                        IsPublished = r.IsPublished,
                        CreatedAt = r.CreatedAt,
                        Media = r.Media,
                        AuthorName = u.Name
                    })
                .ToList();
            return Ok(favorites);
        }
        
        [HttpPost]
        public IActionResult AddToFavorites( FavoriteRequestDTO dto)
        {
            var exists = _context.Favorites.Any(f => f.UserId == dto.UserId && f.RecipeId == dto.RecipeId);
            if (exists)
            {
                return Conflict("Рецепт вже у обраному.");
            }
            var favorite = new Favorite
            {
                UserId = dto.UserId,
                RecipeId = dto.RecipeId
            };
            _context.Favorites.Add(favorite);
            _context.SaveChanges();
            return Created($"/api/favorites/{dto.UserId}", favorite);
        }
    }
}