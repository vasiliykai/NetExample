using System.Security.Claims;
using JWTAuth.Data;
using JWTAuth.Helpers;
using JWTAuth.Models.DTOs;
using JWTAuth.Models.Entities;
using JWTAuth.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JWTAuth.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly JwtTokenHelper _jwtTokenHelper;
    private readonly ApplicationDbContext _context;

    public AuthService(JwtTokenHelper jwtTokenHelper, ApplicationDbContext context)
    {
        _jwtTokenHelper = jwtTokenHelper;
        _context = context;
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest request)
    {
        // Validate user credentials (example: simple username/password check)
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == request.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password");

        // Generate tokens
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var accessToken = _jwtTokenHelper.GenerateAccessToken(user.Id.ToString(), claims);
        var refreshToken = _jwtTokenHelper.GenerateRefreshToken();

        // Save refresh token to database
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7) // Example: 7 days
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
    {
        // Validate the refresh token
        var tokenEntity = await _context.RefreshTokens
            .Include(rt => rt.User)
            .SingleOrDefaultAsync(rt => rt.Token == refreshToken);

        if (tokenEntity == null || tokenEntity.ExpiryDate <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        // Generate new tokens
        var user = tokenEntity.User;
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var newAccessToken = _jwtTokenHelper.GenerateAccessToken(user.Id.ToString(), claims);
        var newRefreshToken = _jwtTokenHelper.GenerateRefreshToken();

        // Update refresh token in database
        tokenEntity.Token = newRefreshToken;
        tokenEntity.ExpiryDate = DateTime.UtcNow.AddDays(7);
        _context.RefreshTokens.Update(tokenEntity);
        await _context.SaveChangesAsync();

        return new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        // Remove the refresh token from database
        var tokenEntity = await _context.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == refreshToken);
        if (tokenEntity != null)
        {
            _context.RefreshTokens.Remove(tokenEntity);
            await _context.SaveChangesAsync();
        }
    }

	public async Task<bool> RegisterAsync(RegisterRequest request)
	{
		// Check if username or email already exists
		if (await _context.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email))
			return false;

		// Create a new user
		var user = new User
		{
			Username = request.Username,
			Email = request.Email,
			PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password) // Secure password hashing
		};

		_context.Users.Add(user);
		await _context.SaveChangesAsync();

		return true;
	}
}
