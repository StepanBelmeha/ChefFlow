namespace ChefFlow.API.Controllers
{
    using ChefFlow.API.Data;
    using ChefFlow.API.DTO;
    using ChefFlow.API.Models;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class RecipeController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        
        [HttpGet]
        public IActionResult GetAll()
        {
            var recipes = _context.Recipes
                .Select(r => new RecipeResponseDTO
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    Title = r.Title,
                    Description = r.Description,
                    Instructions = r.Instructions,
                    IsPublished = r.IsPublished,
                    CreatedAt = r.CreatedAt,
                    Media = r.Media,
                    AuthorName = r.User.Name
                })
                .ToList();
            return Ok(recipes);
        }
        [HttpPost]
        public IActionResult Create(CreateRecipeDTO dto)
        {
            var recipe = new Recipe
            {
                UserId = dto.UserId,
                Title = dto.Title,
                Description = dto.Description,
                Instructions = dto.Instructions,
                IsPublished = dto.IsPublished,
                CreatedAt = DateTime.UtcNow,
                Media = dto.Media
            };
            _context.Recipes.Add(recipe);
            _context.SaveChanges();

            foreach (var ing in dto.Ingredients)
            {
                var product = _context.Products
                    .FirstOrDefault(p => p.Name == ing.ProductName);

                if (product == null)
                {
                    product = new Product { Name = ing.ProductName };
                    _context.Products.Add(product);
                    _context.SaveChanges();
                }

                _context.Ingredients.Add(new Ingredient
                {
                    RecipeId = recipe.Id,
                    ProductId = product.Id,
                    Quantity = ing.Quantity,
                    Unit = ing.Unit
                });
            }
    
            _context.SaveChanges();
            var authorName = _context.Users.Find(dto.UserId)?.Name ?? string.Empty;
            return Created($"/api/recipe/{recipe.Id}", new RecipeResponseDTO
            {
                Id = recipe.Id,
                UserId = recipe.UserId,
                Title = recipe.Title,
                Description = recipe.Description,
                Instructions = recipe.Instructions,
                IsPublished = recipe.IsPublished,
                CreatedAt = recipe.CreatedAt,
                Media = recipe.Media,
                AuthorName = authorName
            });
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadMedia(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не обрано");

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "video/mp4" };
            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest("Непідтримуваний формат файлу");

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "recipes");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { path = $"/images/recipes/{fileName}" });
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetByUserId(int userId, [FromQuery] string? search)
        {
            var query = _context.Recipes
                .Where(r => r.UserId == userId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim();
                query = query.Where(r => r.Title.Contains(normalizedSearch)
                    || r.Description.Contains(normalizedSearch));
            }

            var recipes = query
                .Select(r => new RecipeResponseDTO
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    Title = r.Title,
                    Description = r.Description,
                    Instructions = r.Instructions,
                    IsPublished = r.IsPublished,
                    CreatedAt = r.CreatedAt,
                    Media = r.Media,
                    AuthorName = r.User.Name
                })
                .ToList();
            return Ok(recipes);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var recipe = _context.Recipes
                .Where(r => r.Id == id)
                .Select(r => new RecipeResponseDTO
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    Title = r.Title,
                    Description = r.Description,
                    Instructions = r.Instructions,
                    IsPublished = r.IsPublished,
                    CreatedAt = r.CreatedAt,
                    Media = r.Media,
                    AuthorName = r.User.Name
                })
                .FirstOrDefault();
            if (recipe == null)
            {
                return NotFound();
            }
            return Ok(recipe);
        }

        [HttpGet("{id}/ingredients")]
        public IActionResult GetIngredients(int id)
        {
            var ingredients = _context.Ingredients
                .Where(i => i.RecipeId == id)
                .Join(_context.Products,
                    i => i.ProductId,
                    p => p.Id,
                    (i, p) => new
                    {
                        ProductName = p.Name,
                        i.Quantity,
                        i.Unit
                    })
                .ToList();

            return Ok(ingredients);
        }
        [HttpGet("published")]
        public IActionResult GetPublishedRecipes()
        {
            var recipes = _context.Recipes
                .Where(r => r.IsPublished)
                .Select(r => new RecipeResponseDTO
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    Title = r.Title,
                    Description = r.Description,
                    Instructions = r.Instructions,
                    IsPublished = r.IsPublished,
                    CreatedAt = r.CreatedAt,
                    Media = r.Media,
                    AuthorName = r.User.Name
                })
                .ToList();

            return Ok(recipes);
        }

        [HttpPatch("{id}/publish")]
        public IActionResult Publish(int id)
        {
            var recipe = _context.Recipes.Find(id);
            if (recipe == null) return NotFound();

            recipe.IsPublished = true;
            _context.SaveChanges();

            return Ok(recipe);
        }
    }
}