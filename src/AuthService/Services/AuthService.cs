using AuthService.Data;
using AuthService.DTO;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;
using AuthService.Configuration;
using Microsoft.Extensions.Options;

namespace AuthService.Services
{
    public class AuthService(AuthDbContext context, TokenService tokenService, ILogger<AuthService> logger, IOptions<JwtOptions> jwtOptions)
    {
        private readonly AuthDbContext _context = context;
        private readonly TokenService _tokenService = tokenService;
        private readonly ILogger<AuthService> _logger = logger;
        private readonly JwtOptions _options = jwtOptions.Value;
        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                logger.LogWarning("Спроба реєстрації з існуючим email: {Email}", dto.Email);
                return null;
            }

            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole == null) return null;

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = userRole.Id });
            await _context.SaveChangesAsync();

            // Завантажити з ролями для токена
            user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstAsync(u => u.Id == user.Id);

            logger.LogInformation("Зареєстровано нового користувача: {Email}", user.Email);
            return await CreateTokenPairAsync(user);
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                logger.LogWarning("Невдала спроба входу для email: {Email}", dto.Email);
                return null;
            }

            if (!user.IsActive)
            {
                logger.LogWarning("Спроба входу для неактивного користувача: {Email}", dto.Email);
                return null;
            }

            logger.LogInformation("Успішний вхід: {Email}", user.Email);
            return await CreateTokenPairAsync(user);
        }

        public async Task<AuthResponseDto?> RefreshAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token == null || !token.IsActive)
            {
                logger.LogWarning("Невалідний refresh токен");
                return null;
            }

            // Ротація токена
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            token.RevokedAt = DateTime.UtcNow;
            token.ReplacedByToken = newRefreshToken;

            var newToken = new RefreshToken
            {
                UserId = token.UserId,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(newToken);
            await _context.SaveChangesAsync();

            var accessToken = _tokenService.GenerateAccessToken(token.User);
            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token == null || !token.IsActive) return false;

            token.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            logger.LogInformation("Відкликано refresh токен");
            return true;
        }

        private async Task<AuthResponseDto> CreateTokenPairAsync(User user)
        {
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenDays),
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}