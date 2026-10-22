using Microsoft.EntityFrameworkCore;
using RecipeService.Data;
using RecipeService.DTO;
using RecipeService.Models;

namespace RecipeService.Services
{
    public class RecipeService(RecipeDbContext context, ILogger<RecipeService> logger)
    {
        private readonly RecipeDbContext _context = context;
        private readonly ILogger<RecipeService> _logger = logger;

        public async Task<PagedResult<RecipeDto>> GetAllAsync(
            int page, int pageSize, string? search, bool? isPublished, string? sortBy, bool desc)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.Recipes.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(r => r.Title.Contains(search));

            if (isPublished.HasValue)
                query = query.Where(r => r.IsPublished == isPublished.Value);

            query = sortBy switch
            {
                "title" => desc ? query.OrderByDescending(r => r.Title)
                                : query.OrderBy(r => r.Title),
                "createdAt" => desc ? query.OrderByDescending(r => r.CreatedAt)
                                    : query.OrderBy(r => r.CreatedAt),
                _ => query.OrderByDescending(r => r.CreatedAt)
            };

            var total = await query.CountAsync();
            var recipes = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RecipeDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    Title = r.Title,
                    Description = r.Description,
                    IsPublished = r.IsPublished,
                    CreatedAt = r.CreatedAt,
                    Media = r.Media
                })
                .ToListAsync();

            return new PagedResult<RecipeDto>
            {
                Items = recipes,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        public async Task<RecipeDetailsDto?> GetByIdAsync(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Ingredients)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null) return null;

            return new RecipeDetailsDto
            {
                Id = recipe.Id,
                UserId = recipe.UserId,
                Title = recipe.Title,
                Description = recipe.Description,
                Instructions = recipe.Instructions,
                IsPublished = recipe.IsPublished,
                CreatedAt = recipe.CreatedAt,
                Media = recipe.Media,
                Ingredients = recipe.Ingredients.Select(i => new IngredientDto
                {
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    Unit = i.Unit
                }).ToList()
            };
        }

        public async Task<RecipeDto> CreateAsync(int userId, CreateRecipeDto dto)
        {
            var recipe = new Recipe
            {
                UserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                Instructions = dto.Instructions,
                IsPublished = dto.IsPublished,
                Media = dto.Media,
                CreatedAt = DateTime.UtcNow
            };

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            foreach (var ing in dto.Ingredients)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Name == ing.ProductName);

                if (product == null)
                {
                    product = new Product { Name = ing.ProductName };
                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();
                }

                _context.Ingredients.Add(new Ingredient
                {
                    RecipeId = recipe.Id,
                    ProductId = product.Id,
                    Quantity = ing.Quantity,
                    Unit = ing.Unit
                });
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Створено рецепт: {Title} для userId: {UserId}",
                recipe.Title, userId);

            return new RecipeDto
            {
                Id = recipe.Id,
                UserId = recipe.UserId,
                Title = recipe.Title,
                Description = recipe.Description,
                IsPublished = recipe.IsPublished,
                CreatedAt = recipe.CreatedAt,
                Media = recipe.Media
            };
        }

        public async Task<bool> UpdateAsync(int id, int userId, UpdateRecipeDto dto)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null) return false;

            recipe.Title = dto.Title;
            recipe.Description = dto.Description;
            recipe.Instructions = dto.Instructions;
            recipe.IsPublished = dto.IsPublished;
            recipe.Media = dto.Media;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Оновлено рецепт: {Id}", id);
            return true;
        }

        public async Task<bool> PublishAsync(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null) return false;

            recipe.IsPublished = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Опубліковано рецепт: {Id}", id);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null) return false;

            recipe.IsPublished = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Видалено (м'яке) рецепт: {Id}", id);
            return true;
        }
    }
}