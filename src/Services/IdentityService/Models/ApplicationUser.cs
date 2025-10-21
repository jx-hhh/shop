using Microsoft.AspNetCore.Identity;

namespace IdentityService.Models;

/// <summary>
/// 应用程序用户
/// </summary>
public class ApplicationUser : IdentityUser
{
    public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

/// <summary>
/// 刷新令牌
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public long ExpiresAt { get; set; } // 秒级时间戳
    public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public bool IsRevoked { get; set; }
    public ApplicationUser User { get; set; } = null!;
}
