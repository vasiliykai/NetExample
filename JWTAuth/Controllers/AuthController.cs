using JWTAuth.Models.DTOs;
using JWTAuth.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JWTAuth.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var tokens = await _authService.LoginAsync(request);
        return Ok(tokens);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        var tokens = await _authService.RefreshTokenAsync(refreshToken);
        return Ok(tokens);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        await _authService.LogoutAsync(refreshToken);
        return NoContent();
    
	}
	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegisterRequest request)
	{
		var result = await _authService.RegisterAsync(request);
		if (!result)
			return BadRequest("Registration failed. Username or Email might already exist.");

		return Ok("Registration successful!");
	}
}