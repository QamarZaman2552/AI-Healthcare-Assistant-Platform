using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AIHealthcareAssistant.Application.Common.Interfaces;
using AIHealthcareAssistant.Application.Features.Auth;
using AIHealthcareAssistant.Domain.Entities;
using AIHealthcareAssistant.Domain.Enums;
using AIHealthcareAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AIHealthcareAssistant.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(
        AppDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = HashPassword(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),

            // Public registration always creates a Patient.
            Role = UserRole.Patient,

            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        _context.Patients.Add(new Patient
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return GenerateToken(user);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null ||
            !VerifyPassword(
                request.Password,
                user.PasswordHash))
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "This account is inactive.");
        }

        user.LastLoginAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return GenerateToken(user);
    }

    private AuthResponse GenerateToken(User user)
    {
        var jwtSettings = _configuration
            .GetSection("JwtSettings")
            .Get<JwtSettings>()!;

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Key));

        var expiration = DateTime.UtcNow.AddMinutes(
            jwtSettings.ExpirationInMinutes);

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new(
                ClaimTypes.Email,
                user.Email),

            new(
                ClaimTypes.Name,
                $"{user.FirstName} {user.LastName}"),

            new(
                ClaimTypes.Role,
                user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: expiration,
            signingCredentials:
                new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256)
        );

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler()
                .WriteToken(token),

            Email = user.Email,

            FullName =
                $"{user.FirstName} {user.LastName}",

            Role = user.Role.ToString(),

            Expiration = expiration
        };
    }

    private static string HashPassword(string password)
    {
        using var hmac = new HMACSHA256();
        var salt = hmac.Key;
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32);
        var hashBytes = new byte[64];
        Array.Copy(salt, 0, hashBytes, 0, 32);
        Array.Copy(hash, 0, hashBytes, 32, 32);
        return Convert.ToBase64String(hashBytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        var hashBytes = Convert.FromBase64String(hash);
        var salt = new byte[32];
        Array.Copy(hashBytes, 0, salt, 0, 32);
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
        var computedHash = pbkdf2.GetBytes(32);
        for (int i = 0; i < 32; i++)
        {
            if (hashBytes[i + 32] != computedHash[i]) return false;
        }
        return true;
    }
}