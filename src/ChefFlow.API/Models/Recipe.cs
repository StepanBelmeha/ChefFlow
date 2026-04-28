namespace ChefFlow.API.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Media { get; set; } = string.Empty;

        public User User { get; set; } = null!;
        public ICollection<Note> Notes { get; set; } = new List<Note>();
        public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}