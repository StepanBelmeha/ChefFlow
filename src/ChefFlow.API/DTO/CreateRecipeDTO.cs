using System.ComponentModel.DataAnnotations;
namespace ChefFlow.API.DTO
{
    public class CreateRecipeDTO
    {
         public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Media { get; set; } = string.Empty;
         public List<CreateIngredientDTO> Ingredients { get; set; } = new();
    }
}