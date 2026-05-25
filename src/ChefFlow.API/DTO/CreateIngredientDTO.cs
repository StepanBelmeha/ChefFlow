using System.ComponentModel.DataAnnotations;
namespace ChefFlow.API.DTO
{
    public class CreateIngredientDTO
    {
    public string ProductName { get; set; } = string.Empty;
    public float Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    }
}