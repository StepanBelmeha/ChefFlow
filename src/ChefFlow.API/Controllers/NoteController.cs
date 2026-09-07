using static System.Runtime.InteropServices.JavaScript.JSType;
namespace ChefFlow.API.Controllers
{
    using ChefFlow.API.Data;
    using ChefFlow.API.DTO;
    using ChefFlow.API.Models;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class NoteController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        [HttpGet("recipe/{recipeId}")]
        public IActionResult GetByRecipeId(int recipeId)
        {
            var notes = context.Notes
                .Where(n => n.RecipeId == recipeId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
            if (notes == null)
            {
                return NotFound();
            }
            return Ok(notes);
        }

        [HttpPost]
        public IActionResult Create(CreateNoteDTO dto)
        {
            var note = new Note
            {
                RecipeId = dto.RecipeId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };
            context.Notes.Add(note);
            context.SaveChanges();
            return Ok(note);
        }

    }
}