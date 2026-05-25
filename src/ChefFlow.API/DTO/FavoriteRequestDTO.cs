using System.ComponentModel.DataAnnotations;
namespace ChefFlow.API.DTO
{
    public class FavoriteRequestDTO
    {
        public int UserId { get; set; }

        public int RecipeId { get; set; }
    }
}