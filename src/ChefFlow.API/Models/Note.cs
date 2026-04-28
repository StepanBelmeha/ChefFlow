namespace ChefFlow.API.Models
{
    public class Note
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public Recipe Recipe { get; set; } = null!;
    }
}
