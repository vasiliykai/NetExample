
namespace JWTAuth.Models.Entities;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }

    // Navigation property for Refresh Tokens
    public ICollection<RefreshToken> RefreshTokens { get; set; }
}