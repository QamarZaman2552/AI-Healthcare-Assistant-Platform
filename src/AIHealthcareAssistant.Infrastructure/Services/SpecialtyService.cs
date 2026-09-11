using AIHealthcareAssistant.Application.Features.Specialties;
using AIHealthcareAssistant.Domain.Entities;
using AIHealthcareAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIHealthcareAssistant.Infrastructure.Services;

public class SpecialtyService : ISpecialtyService
{
    private readonly AppDbContext _context;

    public SpecialtyService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SpecialtyResponse> CreateAsync(CreateSpecialtyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Specialty name is required");

        if (await _context.Specialties.AnyAsync(s => s.Name == request.Name))
            throw new InvalidOperationException($"A specialty named '{request.Name}' already exists");

        var specialty = new Specialty
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description
        };

        _context.Specialties.Add(specialty);
        await _context.SaveChangesAsync();

        return ToResponse(specialty);
    }

    public async Task<SpecialtyResponse> UpdateAsync(Guid id, UpdateSpecialtyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Specialty name is required");

        var specialty = await _context.Specialties.FindAsync(id)
            ?? throw new KeyNotFoundException("Specialty not found");

        if (await _context.Specialties.AnyAsync(s => s.Id != id && s.Name == request.Name))
            throw new InvalidOperationException($"A specialty named '{request.Name}' already exists");

        specialty.Name = request.Name.Trim();
        specialty.Description = request.Description;

        await _context.SaveChangesAsync();

        return ToResponse(specialty);
    }

    public async Task<SpecialtyResponse?> GetByIdAsync(Guid id)
    {
        var specialty = await _context.Specialties.FindAsync(id);
        return specialty == null ? null : ToResponse(specialty);
    }

    public async Task<List<SpecialtyResponse>> GetAllAsync()
    {
        return await _context.Specialties
            .OrderBy(s => s.Name)
            .Select(s => new SpecialtyResponse { Id = s.Id, Name = s.Name, Description = s.Description })
            .ToListAsync();
    }

    private static SpecialtyResponse ToResponse(Specialty specialty) => new()
    {
        Id = specialty.Id,
        Name = specialty.Name,
        Description = specialty.Description
    };
}
