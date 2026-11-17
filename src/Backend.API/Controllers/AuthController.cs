using Backend.API.Models;
using Backend.Application.DTOs.Auth;
using Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Controllers;

public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        if (response is null)
        {
            return Unauthorized(ApiResponse.Fail("Invalid email or password.", HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<LoginResponse>.Ok(response, "Login successful."));
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirst("UserId")?.Value;
        var fullName = User.FindFirst("FullName")?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        return Ok(ApiResponse<object>.Ok(new { userId, fullName, email, role }, "Authenticated."));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public IActionResult AdminOnly() =>
        Ok(ApiResponse<string>.Ok("Admin endpoint access granted.", "Authorized."));

    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpGet("manager-or-admin")]
    public IActionResult ManagerOrAdmin() =>
        Ok(ApiResponse<string>.Ok("Manager or admin access granted.", "Authorized."));
}
