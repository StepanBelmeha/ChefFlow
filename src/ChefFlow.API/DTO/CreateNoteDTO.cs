namespace ChefFlow.API.DTO
{
    public class CreateNoteDTO
    {
        public int RecipeId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}