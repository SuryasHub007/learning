using Backend.Domain.Entities;

namespace Backend.Application.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
