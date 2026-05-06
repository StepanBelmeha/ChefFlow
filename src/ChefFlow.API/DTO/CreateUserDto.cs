using ChefFlow.API.Models;
namespace ChefFlow.API.DTO
{
    public class CreateUserDTO
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}