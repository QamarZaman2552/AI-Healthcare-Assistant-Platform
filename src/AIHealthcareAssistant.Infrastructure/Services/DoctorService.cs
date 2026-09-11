using AIHealthcareAssistant.Application.Common.Models;
using AIHealthcareAssistant.Application.Features.Doctors;
using AIHealthcareAssistant.Application.Features.Specialties;
using AIHealthcareAssistant.Domain.Entities;
using AIHealthcareAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIHealthcareAssistant.Infrastructure.Services;

public class DoctorService : IDoctorService
{
    private readonly AppDbContext _context;

    public DoctorService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DoctorResponse> CreateAsync(CreateDoctorRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LicenseNumber))
            throw new ArgumentException("License number is required");

        var user = await _context.Users.FindAsync(request.UserId)
            ?? throw new KeyNotFoundException("User not found");

        if (user.Role != Domain.Enums.UserRole.Doctor)
            throw new ArgumentException("User does not have the Doctor role");

        if (await _context.Doctors.AnyAsync(d => d.LicenseNumber == request.LicenseNumber))
            throw new InvalidOperationException($"License number '{request.LicenseNumber}' is already registered");

        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == request.UserId);
        if (doctor == null)
        {
            doctor = new Doctor { Id = Guid.NewGuid(), UserId = request.UserId };
            _context.Doctors.Add(doctor);
        }
        else if (!string.IsNullOrEmpty(doctor.LicenseNumber))
        {
            throw new InvalidOperationException("A doctor profile already exists for this user");
        }

        doctor.LicenseNumber = request.LicenseNumber.Trim();
        doctor.YearsOfExperience = request.YearsOfExperience;
        doctor.Biography = request.Biography;
        doctor.ConsultationFee = request.ConsultationFee;
        doctor.ClinicName = request.ClinicName;
        doctor.ClinicAddress = request.ClinicAddress;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(doctor.Id) ?? throw new InvalidOperationException("Failed to load created doctor");
    }

    public async Task<DoctorResponse> UpdateAsync(Guid id, UpdateDoctorRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LicenseNumber))
            throw new ArgumentException("License number is required");

        var doctor = await _context.Doctors.FindAsync(id)
            ?? throw new KeyNotFoundException("Doctor not found");

        if (await _context.Doctors.AnyAsync(d => d.Id != id && d.LicenseNumber == request.LicenseNumber))
            throw new InvalidOperationException($"License number '{request.LicenseNumber}' is already registered");

        doctor.LicenseNumber = request.LicenseNumber.Trim();
        doctor.YearsOfExperience = request.YearsOfExperience;
        doctor.Biography = request.Biography;
        doctor.ConsultationFee = request.ConsultationFee;
        doctor.ClinicName = request.ClinicName;
        doctor.ClinicAddress = request.ClinicAddress;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id) ?? throw new InvalidOperationException("Failed to load updated doctor");
    }

    public async Task<DoctorResponse?> GetByIdAsync(Guid id)
    {
        var doctor = await BaseQuery().FirstOrDefaultAsync(d => d.Id == id);
        return doctor == null ? null : ToResponse(doctor);
    }

    public async Task<DoctorResponse?> GetByUserIdAsync(Guid userId)
    {
        var doctor = await BaseQuery().FirstOrDefaultAsync(d => d.UserId == userId);
        return doctor == null ? null : ToResponse(doctor);
    }

    public async Task<PagedResult<DoctorResponse>> GetDoctorsAsync(DoctorQuery query)
    {
        var doctors = BaseQuery();

        if (query.SpecialtyId.HasValue)
        {
            doctors = doctors.Where(d => d.DoctorSpecialties.Any(ds => ds.SpecialtyId == query.SpecialtyId.Value));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            doctors = doctors.Where(d =>
                EF.Functions.Like(d.User.FirstName, $"%{term}%") ||
                EF.Functions.Like(d.User.LastName, $"%{term}%"));
        }

        if (query.AvailableOnly == true)
        {
            doctors = doctors.Where(d => d.Availabilities.Any(a => a.IsActive));
        }

        if (query.IsActive.HasValue)
        {
            doctors = doctors.Where(d => d.IsActive == query.IsActive.Value);
        }

        if (query.IsVerified.HasValue)
        {
            doctors = doctors.Where(d => d.IsVerified == query.IsVerified.Value);
        }

        var totalCount = await doctors.CountAsync();

        var items = await doctors
            .OrderBy(d => d.User.FirstName).ThenBy(d => d.User.LastName)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<DoctorResponse>
        {
            Items = items.Select(ToResponse).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<DoctorResponse> UpdateStatusAsync(Guid id, bool isActive)
    {
        var doctor = await _context.Doctors.FindAsync(id)
            ?? throw new KeyNotFoundException("Doctor not found");

        doctor.IsActive = isActive;
        await _context.SaveChangesAsync();

        return await GetByIdAsync(id) ?? throw new InvalidOperationException("Failed to load updated doctor");
    }

    public async Task<DoctorResponse> UpdateVerificationAsync(Guid id, bool isVerified)
    {
        var doctor = await _context.Doctors.FindAsync(id)
            ?? throw new KeyNotFoundException("Doctor not found");

        doctor.IsVerified = isVerified;
        await _context.SaveChangesAsync();

        return await GetByIdAsync(id) ?? throw new InvalidOperationException("Failed to load updated doctor");
    }

    public async Task<SpecialtyResponse> AssignSpecialtyAsync(Guid doctorId, AssignSpecialtyRequest request)
    {
        var doctor = await _context.Doctors.FindAsync(doctorId)
            ?? throw new KeyNotFoundException("Doctor not found");

        var specialty = await _context.Specialties.FindAsync(request.SpecialtyId)
            ?? throw new KeyNotFoundException("Specialty not found");

        if (await _context.DoctorSpecialties.AnyAsync(ds => ds.DoctorId == doctorId && ds.SpecialtyId == request.SpecialtyId))
            throw new InvalidOperationException("This specialty is already assigned to the doctor");

        _context.DoctorSpecialties.Add(new DoctorSpecialty
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorId,
            SpecialtyId = request.SpecialtyId,
            IsPrimary = request.IsPrimary
        });

        await _context.SaveChangesAsync();

        return new SpecialtyResponse { Id = specialty.Id, Name = specialty.Name, Description = specialty.Description };
    }

    public async Task RemoveSpecialtyAsync(Guid doctorId, Guid specialtyId)
    {
        var mapping = await _context.DoctorSpecialties
            .FirstOrDefaultAsync(ds => ds.DoctorId == doctorId && ds.SpecialtyId == specialtyId)
            ?? throw new KeyNotFoundException("This specialty is not assigned to the doctor");

        _context.DoctorSpecialties.Remove(mapping);
        await _context.SaveChangesAsync();
    }

    public async Task<List<SpecialtyResponse>> GetSpecialtiesByDoctorAsync(Guid doctorId)
    {
        if (!await _context.Doctors.AnyAsync(d => d.Id == doctorId))
            throw new KeyNotFoundException("Doctor not found");

        return await _context.DoctorSpecialties
            .Where(ds => ds.DoctorId == doctorId)
            .Select(ds => new SpecialtyResponse
            {
                Id = ds.Specialty.Id,
                Name = ds.Specialty.Name,
                Description = ds.Specialty.Description
            })
            .ToListAsync();
    }

    public async Task<List<DoctorResponse>> GetDoctorsBySpecialtyAsync(Guid specialtyId)
    {
        if (!await _context.Specialties.AnyAsync(s => s.Id == specialtyId))
            throw new KeyNotFoundException("Specialty not found");

        var doctors = await BaseQuery()
            .Where(d => d.DoctorSpecialties.Any(ds => ds.SpecialtyId == specialtyId))
            .OrderBy(d => d.User.FirstName).ThenBy(d => d.User.LastName)
            .ToListAsync();

        return doctors.Select(ToResponse).ToList();
    }

    private IQueryable<Doctor> BaseQuery() =>
        _context.Doctors
            .Include(d => d.User)
            .Include(d => d.DoctorSpecialties).ThenInclude(ds => ds.Specialty)
            .Include(d => d.Availabilities)
            .AsQueryable();

    private static DoctorResponse ToResponse(Doctor doctor) => new()
    {
        Id = doctor.Id,
        UserId = doctor.UserId,
        FullName = $"{doctor.User.FirstName} {doctor.User.LastName}",
        Email = doctor.User.Email,
        PhoneNumber = doctor.User.PhoneNumber,
        LicenseNumber = doctor.LicenseNumber,
        YearsOfExperience = doctor.YearsOfExperience,
        Biography = doctor.Biography,
        ConsultationFee = doctor.ConsultationFee,
        ClinicName = doctor.ClinicName,
        ClinicAddress = doctor.ClinicAddress,
        IsActive = doctor.IsActive,
        IsVerified = doctor.IsVerified,
        Specialties = doctor.DoctorSpecialties
            .Select(ds => new SpecialtyResponse { Id = ds.Specialty.Id, Name = ds.Specialty.Name, Description = ds.Specialty.Description })
            .ToList(),
        CreatedAt = doctor.CreatedAt
    };
}
