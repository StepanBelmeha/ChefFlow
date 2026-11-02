namespace ApiGateway.Dtos
{
    public class RecipeScreenDto
    {
        public RecipeInfoDto? Recipe { get; set; }
        public List<TaskInfoDto>? MyTasks { get; set; }
        public List<string> UnavailableParts { get; set; } = new();
    }

    public class RecipeInfoDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public string? Media { get; set; }
        public List<IngredientInfoDto> Ingredients { get; set; } = new();
    }

    public class IngredientInfoDto
    {
        public string ProductName { get; set; } = string.Empty;
        public float Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    public class TaskInfoDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        public string Priority { get; set; } = string.Empty;
    }
}
