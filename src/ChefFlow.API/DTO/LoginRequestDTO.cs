using System.ComponentModel.DataAnnotations;
namespace ChefFlow.API.DTO
{
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "Email обов'язковий")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Пароль обов'язковий")]
        public string Password { get; set; } = string.Empty;
    }
}