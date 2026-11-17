using Microsoft.AspNetCore.Identity;

namespace Backend.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<object> _hasher = new();

    public string HashPassword(string password) => _hasher.HashPassword(new object(), password);

    public bool VerifyPassword(string passwordHash, string password)
    {
        var result = _hasher.VerifyHashedPassword(new object(), passwordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
