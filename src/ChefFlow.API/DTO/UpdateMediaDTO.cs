using System.ComponentModel.DataAnnotations;
namespace ChefFlow.API.DTO
{
    public class UpdateMediaDTO
    {
        [Required]
        public string MediaPath { get; set; } = string.Empty;
    }
}
