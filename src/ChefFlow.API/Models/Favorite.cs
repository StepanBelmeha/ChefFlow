namespace ChefFlow.API.Models
{
    public class Favorite
    {
        public int UserId { get; set; }
        public int RecipeId { get; set; }

        public User User { get; set; } = null!;
        public Recipe Recipe { get; set; } = null!;
    }
}
