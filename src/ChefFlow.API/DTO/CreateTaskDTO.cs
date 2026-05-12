using System.ComponentModel.DataAnnotations;
namespace ChefFlow.API.DTO
{
    public class CreateTaskDTO
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Назва обов'язкова")]
        [MaxLength(100, ErrorMessage = "Назва не більше 100 символів")]
        public string Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }

        [Required(ErrorMessage = "Пріоритет обов'язковий")]
        public string Priority { get; set; }
    }
}