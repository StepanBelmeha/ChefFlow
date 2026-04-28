namespace ChefFlow.API.Models
{
    public class Task
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public string Priority { get; set; } = string.Empty;

        public User User { get; set; } = null!;
    }
}
