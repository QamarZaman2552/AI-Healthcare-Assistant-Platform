using System.Security.Cryptography;
using AIHealthcareAssistant.Domain.Entities;
using AIHealthcareAssistant.Domain.Enums;
using AIHealthcareAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIHealthcareAssistant.Infrastructure.Services;

public class DatabaseSeeder
{
    private readonly AppDbContext _context;

    public DatabaseSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.Users.AnyAsync())
            return;

        var admin = CreateUser("admin@healthcare.com", "Admin@123", "System", "Admin", "+10000000001", UserRole.Admin);
        var doctor = CreateUser("doctor@healthcare.com", "Doctor@123", "Test", "Doctor", "+10000000002", UserRole.Doctor);
        var patient = CreateUser("patient@healthcare.com", "Patient@123", "Test", "Patient", "+10000000003", UserRole.Patient);

        _context.Users.AddRange(admin, doctor, patient);

        _context.Doctors.AddRange(new Doctor
        {
            UserId = doctor.Id,
            LicenseNumber = "DOC-001",
            YearsOfExperience = 10,
            IsActive = true,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow
        });

        _context.Patients.AddRange(new Patient
        {
            UserId = patient.Id,
            CreatedAt = DateTime.UtcNow
        });

        _context.AdminUsers.AddRange(new AdminUser
        {
            UserId = admin.Id,
            AccessLevel = AdminAccessLevel.SuperAdmin,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    private static User CreateUser(string email, string password, string firstName, string lastName, string phone, UserRole role)
    {
        return new User
        {
            Email = email,
            PasswordHash = HashPassword(password),
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phone,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(32);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        var result = new byte[64];
        Buffer.BlockCopy(salt, 0, result, 0, 32);
        Buffer.BlockCopy(hash, 0, result, 32, 32);
        return Convert.ToBase64String(result);
    }
}
