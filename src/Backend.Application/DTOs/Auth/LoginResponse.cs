using Backend.Domain.Enums;
using System.Text.Json.Serialization;

namespace Backend.Application.DTOs.Auth;

public class LoginResponse
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserRole Role { get; init; }
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
}
