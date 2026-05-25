using System.ComponentModel.DataAnnotations;
using ChefFlow.API.Models;
namespace ChefFlow.API.DTO
{
    public class CreateUserDTO
    {
        [Required(ErrorMessage = "Ім'я обов'язкове")]
        public required string Name { get; set; }
        [Required(ErrorMessage = "Email обов'язковий")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Пароль обов'язковий")]
        [MinLength(8, ErrorMessage = "Пароль має бути не менше 8 символів")]
        public required string Password { get; set; }
    }
}