using JWTAuth.Models.DTOs;

namespace JWTAuth.Services.Interfaces;

public interface IAuthService
{
    Task<TokenResponse> LoginAsync(LoginRequest request);
    Task<TokenResponse> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
	Task<bool> RegisterAsync(RegisterRequest request);
}
