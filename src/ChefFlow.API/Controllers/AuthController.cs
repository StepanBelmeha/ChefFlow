using ChefFlow.API.Data;
using ChefFlow.API.DTO;
using ChefFlow.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChefFlow.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;
        private readonly ILogger _logger;

        public AuthController(AppDbContext context, JwtService jwtService, ILogger<AuthController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                _logger.LogWarning("Невдала спроба входу для email: {Email}", request.Email);
                return Unauthorized("Невірний email або пароль.");
            }

            var token = _jwtService.GenerateToken(user.Id, user.Email);
            _logger.LogInformation("Користувач з email: {Email} успішно увійшов", request.Email);
            return Ok(new
            {
                Token = token,
                Name = user.Name,
                Email = user.Email

            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserDTO request)
        {
            var exists = await _context.Users
                .AnyAsync(u => u.Email == request.Email);

            if (exists)
            {
                _logger.LogWarning("Спроба реєстрації з існуючим email: {Email}", request.Email);
                return BadRequest("Користувач з таким email вже існує.");
            }
            if(string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            {
                _logger.LogWarning("Спроба реєстрації з ненадійним паролем для email: {Email}", request.Email);
                return BadRequest("Пароль повинен бути не менше 8 символів.");
            }

            var user = new ChefFlow.API.Models.User
            {
                Name = request.Name,
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Новий користувач зареєстрований: {Email}", request.Email);  
            return Ok(new { Message = "Реєстрація успішна." });
        }
    }
}