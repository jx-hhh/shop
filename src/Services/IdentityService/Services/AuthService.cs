using IdentityService.Data;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Configuration;
using Shared.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Services;

public interface IAuthService
{
    Task<ApiResponse<LoginResponse>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<LoginResponse>> RefreshTokenAsync(string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _context = context;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<ApiResponse<LoginResponse>> RegisterAsync(RegisterRequest request)
    {
        // 验证密码匹配
        if (request.Password != request.ConfirmPassword)
        {
            return ApiResponse<LoginResponse>.ErrorResponse("Passwords do not match.");
        }

        // 检查用户名是否已存在
        var existingUser = await _userManager.FindByNameAsync(request.Username);
        if (existingUser != null)
        {
            return ApiResponse<LoginResponse>.ErrorResponse("Username already exists.");
        }

        // 检查邮箱是否已存在
        existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return ApiResponse<LoginResponse>.ErrorResponse("Email already exists.");
        }

        // 创建新用户
        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return ApiResponse<LoginResponse>.ErrorResponse(
                "Failed to create user.",
                result.Errors.Select(e => e.Description).ToList());
        }

        // 生成令牌并返回
        var loginResponse = await GenerateTokensAsync(user);
        return ApiResponse<LoginResponse>.SuccessResponse(loginResponse, "User registered successfully.");
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        // 查找用户
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return ApiResponse<LoginResponse>.ErrorResponse("Invalid email or password.");
        }

        // 验证密码
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return ApiResponse<LoginResponse>.ErrorResponse("Invalid email or password.");
        }

        // 生成令牌并返回
        var loginResponse = await GenerateTokensAsync(user);
        return ApiResponse<LoginResponse>.SuccessResponse(loginResponse, "Login successful.");
    }

    public async Task<ApiResponse<LoginResponse>> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null || storedToken.IsRevoked)
        {
            return ApiResponse<LoginResponse>.ErrorResponse("Invalid refresh token.");
        }

        var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (storedToken.ExpiresAt < currentTimestamp)
        {
            return ApiResponse<LoginResponse>.ErrorResponse("Refresh token has expired.");
        }

        // 撤销旧的 refresh token
        storedToken.IsRevoked = true;
        await _context.SaveChangesAsync();

        // 生成新令牌
        var loginResponse = await GenerateTokensAsync(storedToken.User);
        return ApiResponse<LoginResponse>.SuccessResponse(loginResponse, "Token refreshed successfully.");
    }

    private async Task<LoginResponse> GenerateTokensAsync(ApplicationUser user)
    {
        // 生成 Access Token
        var accessToken = GenerateAccessToken(user);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes).ToUnixTimeSeconds();

        // 生成 Refresh Token
        var refreshToken = GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays).ToUnixTimeSeconds()
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                CreatedAt = user.CreatedAt
            }
        };
    }

    private string GenerateAccessToken(ApplicationUser user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Name, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
