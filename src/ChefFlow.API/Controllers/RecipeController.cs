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
            var recipes = _context
                .Recipes
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

            // Повертай простий об'єкт без циклічних посилань
            return Ok(recipe);
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
        public IActionResult GetByUserId(int userId)
        {
            var recipes = _context.Recipes
            .Where(r => r.UserId == userId)
            .ToList();
            if (recipes == null)
            {
                return NotFound();
            }
            return Ok(recipes);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var recipe = _context.Recipes.Find(id);
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
    }
}