namespace ChefFlow.API.Models
{
    public class Ingredient
    {
        public int RecipeId { get; set; }
        public int ProductId { get; set; }
        public float Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;

        public Recipe Recipe { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}