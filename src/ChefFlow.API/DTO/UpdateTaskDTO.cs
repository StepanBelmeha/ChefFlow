using System.ComponentModel.DataAnnotations;
namespace ChefFlow.API.DTO
{
    public class UpdateTaskDTO
    {
        [Required(ErrorMessage = "Назва обов'язкова")]
        [MaxLength(100, ErrorMessage = "Назва не більше 100 символів")]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public string Priority { get; set; } = string.Empty;
    }
}