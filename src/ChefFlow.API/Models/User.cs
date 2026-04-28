namespace ChefFlow.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
        public ICollection<Task> Tasks { get; set; } = new List<Task>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}