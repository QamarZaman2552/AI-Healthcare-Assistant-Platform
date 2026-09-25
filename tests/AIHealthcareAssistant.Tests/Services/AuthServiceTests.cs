using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AIHealthcareAssistant.Application.Features.Auth;
using AIHealthcareAssistant.Domain.Entities;
using AIHealthcareAssistant.Domain.Enums;
using AIHealthcareAssistant.Infrastructure.Persistence;
using AIHealthcareAssistant.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AIHealthcareAssistant.Tests.Services;

public class AuthServiceTests
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Key"] = "test-key-must-be-at-least-32-characters-long!!!",
                ["JwtSettings:Issuer"] = "AIHealthcareAssistant",
                ["JwtSettings:Audience"] = "AIHealthcareAssistantUsers",
                ["JwtSettings:ExpirationInMinutes"] = "60"
            })
            .Build();

        _authService = new AuthService(_context, _config);
    }

    [Fact]
    public async Task RegisterAsync_ValidPatient_ReturnsAuthResponseWithPatientRole()
    {
        var request = new RegisterRequest
        {
            Email = "patient@test.com",
            Password = "Test@1234",
            FirstName = "Test",
            LastName = "Patient"
        };

        var result = await _authService.RegisterAsync(request);

        Assert.NotNull(result);
        Assert.Equal("patient@test.com", result.Email);
        Assert.Equal("Patient", result.Role);
        Assert.NotEmpty(result.Token);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "patient@test.com");
        Assert.NotNull(user);
        Assert.Equal(UserRole.Patient, user.Role);
    }

    [Fact]
    public async Task RegisterAsync_ValidAdmin_ReturnsAuthResponseWithAdminRole()
    {
        var request = new RegisterRequest
        {
            Email = "admin@test.com",
            Password = "Test@1234",
            FirstName = "Test",
            LastName = "Admin",
            Role = "Admin"
        };

        var result = await _authService.RegisterAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Admin", result.Role);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@test.com");
        Assert.NotNull(user);
        Assert.Equal(UserRole.Admin, user.Role);
    }

    [Fact]
    public async Task RegisterAsync_ValidDoctor_ReturnsAuthResponseWithDoctorRole()
    {
        var request = new RegisterRequest
        {
            Email = "doctor@test.com",
            Password = "Test@1234",
            FirstName = "Test",
            LastName = "Doctor",
            Role = "Doctor"
        };

        var result = await _authService.RegisterAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Doctor", result.Role);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "doctor@test.com");
        Assert.NotNull(user);
        Assert.Equal(UserRole.Doctor, user.Role);
    }

    [Fact]
    public async Task RegisterAsync_InvalidRole_DefaultsToPatient()
    {
        var request = new RegisterRequest
        {
            Email = "random@test.com",
            Password = "Test@1234",
            FirstName = "Test",
            LastName = "User",
            Role = "InvalidRole"
        };

        var result = await _authService.RegisterAsync(request);

        Assert.Equal("Patient", result.Role);
    }

    [Fact]
    public async Task RegisterAsync_NullRole_DefaultsToPatient()
    {
        var request = new RegisterRequest
        {
            Email = "nullrole@test.com",
            Password = "Test@1234",
            FirstName = "Test",
            LastName = "User"
        };

        var result = await _authService.RegisterAsync(request);

        Assert.Equal("Patient", result.Role);
    }

    [Fact]
    public async Task RegisterAsync_EmptyRole_DefaultsToPatient()
    {
        var request = new RegisterRequest
        {
            Email = "emptyrole@test.com",
            Password = "Test@1234",
            FirstName = "Test",
            LastName = "User",
            Role = ""
        };

        var result = await _authService.RegisterAsync(request);

        Assert.Equal("Patient", result.Role);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        var request1 = new RegisterRequest
        {
            Email = "dup@test.com",
            Password = "Test@1234",
            FirstName = "First",
            LastName = "User"
        };
        await _authService.RegisterAsync(request1);

        var request2 = new RegisterRequest
        {
            Email = "dup@test.com",
            Password = "Test@5678",
            FirstName = "Second",
            LastName = "User"
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.RegisterAsync(request2));
        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task RegisterAsync_CreatesPatientProfile()
    {
        var request = new RegisterRequest
        {
            Email = "profile@test.com",
            Password = "Test@1234",
            FirstName = "Test",
            LastName = "Patient"
        };

        await _authService.RegisterAsync(request);

        var patient = await _context.Patients.FirstOrDefaultAsync(p =>
            p.User.Email == "profile@test.com");
        Assert.NotNull(patient);
    }

    [Fact]
    public async Task RegisterAsync_PasswordIsHashed()
    {
        var request = new RegisterRequest
        {
            Email = "hash@test.com",
            Password = "Test@1234",
            FirstName = "Test",
            LastName = "User"
        };

        await _authService.RegisterAsync(request);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "hash@test.com");
        Assert.NotNull(user);
        Assert.NotEmpty(user.PasswordHash);
        Assert.NotEqual("Test@1234", user.PasswordHash);
    }

    [Fact]
    public async Task RegisterAsync_EmailIsLowercase()
    {
        var request = new RegisterRequest
        {
            Email = "UPPER@TEST.COM",
            Password = "Test@1234",
            FirstName = "Test",
            LastName = "User"
        };

        await _authService.RegisterAsync(request);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "upper@test.com");
        Assert.NotNull(user);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        await _context.Users.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = "login@test.com",
            PasswordHash = AuthServiceHashPassword("Correct@1234"),
            Role = UserRole.Patient,
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "login@test.com",
            Password = "Correct@1234"
        });

        Assert.NotNull(result);
        Assert.Equal("login@test.com", result.Email);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task LoginAsync_InvalidEmail_ThrowsUnauthorizedAccessException()
    {
        await _context.Users.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@test.com",
            PasswordHash = AuthServiceHashPassword("Test@1234"),
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(new LoginRequest
            {
                Email = "nonexistent@test.com",
                Password = "Test@1234"
            }));
        Assert.Contains("Invalid email or password", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        await _context.Users.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = "login@test.com",
            PasswordHash = AuthServiceHashPassword("Correct@1234"),
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(new LoginRequest
            {
                Email = "login@test.com",
                Password = "Wrong@1234"
            }));
        Assert.Contains("Invalid email or password", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_InactiveAccount_ThrowsUnauthorizedAccessException()
    {
        await _context.Users.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = "inactive@test.com",
            PasswordHash = AuthServiceHashPassword("Test@1234"),
            IsActive = false
        });
        await _context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(new LoginRequest
            {
                Email = "inactive@test.com",
                Password = "Test@1234"
            }));
        Assert.Contains("inactive", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_UpdatesLastLoginAt()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "lastlogin@test.com",
            PasswordHash = AuthServiceHashPassword("Test@1234"),
            IsActive = true
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        await _authService.LoginAsync(new LoginRequest
        {
            Email = "lastlogin@test.com",
            Password = "Test@1234"
        });

        var updatedUser = await _context.Users.FindAsync(user.Id);
        Assert.NotNull(updatedUser);
        Assert.NotNull(updatedUser.LastLoginAt);
        Assert.True(updatedUser.LastLoginAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_AdminRolePreservedInToken()
    {
        await _context.Users.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = "adminlogin@test.com",
            PasswordHash = AuthServiceHashPassword("Admin@1234"),
            Role = UserRole.Admin,
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "adminlogin@test.com",
            Password = "Admin@1234"
        });

        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public async Task LoginAsync_DoctorRolePreservedInToken()
    {
        await _context.Users.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = "doctorlogin@test.com",
            PasswordHash = AuthServiceHashPassword("Doctor@1234"),
            Role = UserRole.Doctor,
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "doctorlogin@test.com",
            Password = "Doctor@1234"
        });

        Assert.Equal("Doctor", result.Role);
    }

    [Fact]
    public async Task LoginAsync_NullPasswordHash_ThrowsUnauthorized()
    {
        await _context.Users.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = "nulldb@test.com",
            PasswordHash = AuthServiceHashPassword("Test@1234"),
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var nullUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "nulldb@test.com");
        if (nullUser != null) nullUser.PasswordHash = null!;
        await _context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(new LoginRequest
            {
                Email = "nulldb@test.com",
                Password = "Test@1234"
            }));
        Assert.Contains("Invalid email or password", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_EmptyPasswordHash_ThrowsUnauthorized()
    {
        await _context.Users.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = "emptyhash@test.com",
            PasswordHash = AuthServiceHashPassword("Test@1234"),
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var emptyUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "emptyhash@test.com");
        if (emptyUser != null) emptyUser.PasswordHash = "";
        await _context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(new LoginRequest
            {
                Email = "emptyhash@test.com",
                Password = "Test@1234"
            }));
        Assert.Contains("Invalid email or password", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_TokenContainsAllRequiredClaims()
    {
        await _context.Users.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = "claims@test.com",
            PasswordHash = AuthServiceHashPassword("Test@1234"),
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.Patient,
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "claims@test.com",
            Password = "Test@1234"
        });

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.NameIdentifier);
        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Email);
        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Name);
        Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role);
    }

    [Fact]
    public async Task LoginAsync_TokenRoleClaimMatchesUserRole()
    {
        await _context.Users.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = "roleclaim@test.com",
            PasswordHash = AuthServiceHashPassword("Test@1234"),
            Role = UserRole.Admin,
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "roleclaim@test.com",
            Password = "Test@1234"
        });

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);
        var roleClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

        Assert.NotNull(roleClaim);
        Assert.Equal("Admin", roleClaim.Value);
    }

    [Fact]
    public async Task LoginAsync_TokenExpirationIsWithinExpectedRange()
    {
        await _context.Users.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = "expiry@test.com",
            PasswordHash = AuthServiceHashPassword("Test@1234"),
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "expiry@test.com",
            Password = "Test@1234"
        });

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        Assert.True(token.ValidTo > DateTime.UtcNow);
        Assert.True(token.ValidTo < DateTime.UtcNow.AddMinutes(65));
    }

    private static string AuthServiceHashPassword(string password)
    {
        var salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
        var hash = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
            password, salt, 100_000, System.Security.Cryptography.HashAlgorithmName.SHA256, 32);
        var result = new byte[64];
        System.Buffer.BlockCopy(salt, 0, result, 0, 32);
        System.Buffer.BlockCopy(hash, 0, result, 32, 32);
        return Convert.ToBase64String(result);
    }
}
