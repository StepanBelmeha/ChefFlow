namespace ChefFlow.API.DTO
{
    public class UpdateTaskDTO
    {
        public DateTime Deadline { get; set; }
        public string Priority { get; set; } = string.Empty;
    }
}