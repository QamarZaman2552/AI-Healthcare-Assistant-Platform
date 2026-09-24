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
        {
            await SeedSpecialtiesAsync();
            return;
        }

        var admin = CreateUser("admin@healthcare.com", "Admin@123", "System", "Admin", "+10000000001", UserRole.Admin);
        var doctor = CreateUser("doctor@healthcare.com", "Doctor@123", "Test", "Doctor", "+10000000002", UserRole.Doctor);
        var patient = CreateUser("patient@healthcare.com", "Patient@123", "Test", "Patient", "+10000000003", UserRole.Patient);

        _context.Users.AddRange(admin, doctor, patient);
        await _context.SaveChangesAsync();

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
        await SeedSpecialtiesAsync();
    }

    private async Task SeedSpecialtiesAsync()
    {
        if (await _context.Specialties.AnyAsync())
            return;

        var specialties = new[]
        {
            new Specialty { Id = Guid.NewGuid(), Name = "Cardiology", Description = "Heart and cardiovascular system", CreatedAt = DateTime.UtcNow },
            new Specialty { Id = Guid.NewGuid(), Name = "Dermatology", Description = "Skin, hair, and nails", CreatedAt = DateTime.UtcNow },
            new Specialty { Id = Guid.NewGuid(), Name = "Orthopedics", Description = "Musculoskeletal system", CreatedAt = DateTime.UtcNow },
            new Specialty { Id = Guid.NewGuid(), Name = "Pediatrics", Description = "Children's health", CreatedAt = DateTime.UtcNow },
            new Specialty { Id = Guid.NewGuid(), Name = "Neurology", Description = "Nervous system disorders", CreatedAt = DateTime.UtcNow },
            new Specialty { Id = Guid.NewGuid(), Name = "Oncology", Description = "Cancer treatment", CreatedAt = DateTime.UtcNow },
            new Specialty { Id = Guid.NewGuid(), Name = "Psychiatry", Description = "Mental health", CreatedAt = DateTime.UtcNow },
            new Specialty { Id = Guid.NewGuid(), Name = "Radiology", Description = "Medical imaging", CreatedAt = DateTime.UtcNow }
        };

        _context.Specialties.AddRange(specialties);

        var doctor = await _context.Doctors.FirstOrDefaultAsync();
        if (doctor != null)
        {
            _context.DoctorSpecialties.AddRange(specialties.Select(s => new DoctorSpecialty
            {
                Id = Guid.NewGuid(),
                DoctorId = doctor.Id,
                SpecialtyId = s.Id,
                IsPrimary = s.Name == "Cardiology",
                CreatedAt = DateTime.UtcNow
            }));
        }

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
