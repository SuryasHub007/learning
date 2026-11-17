using Backend.Application.DTOs.Auth;
using Backend.Application.Interfaces;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(ApplicationDbContext dbContext, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Email == email, cancellationToken);

        if (user is null || !user.IsActive ||
            !_passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
        {
            return null;
        }

        var (token, expiresAtUtc) = _jwtTokenService.GenerateToken(user);
        return new LoginResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Token = token,
            ExpiresAtUtc = expiresAtUtc
        };
    }
}
