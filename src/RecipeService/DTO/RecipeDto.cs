using System.ComponentModel.DataAnnotations;

namespace RecipeService.DTO
{
    public class CreateRecipeDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Instructions { get; set; } = string.Empty;

        public bool IsPublished { get; set; } = false;

        public string? Media { get; set; }

        public List<CreateIngredientDto> Ingredients { get; set; } = new();
    }

    public class CreateIngredientDto
    {
        [Required]
        public string ProductName { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public float Quantity { get; set; }

        [Required]
        public string Unit { get; set; } = string.Empty;
    }

    public class UpdateRecipeDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Instructions { get; set; } = string.Empty;

        public bool IsPublished { get; set; }

        public string? Media { get; set; }
    }

    public class RecipeDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Media { get; set; }
    }

    public class RecipeDetailsDto : RecipeDto
    {
        public string Instructions { get; set; } = string.Empty;
        public List<IngredientDto> Ingredients { get; set; } = new();
    }

    public class IngredientDto
    {
        public string ProductName { get; set; } = string.Empty;
        public float Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }

}