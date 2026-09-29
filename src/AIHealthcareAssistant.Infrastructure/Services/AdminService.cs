using AIHealthcareAssistant.Application.Common.Interfaces;
using AIHealthcareAssistant.Application.Features.Admin;
using AIHealthcareAssistant.Domain.Entities;
using AIHealthcareAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIHealthcareAssistant.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;
    private readonly IAIService _aiService;

    public AdminService(AppDbContext context, IAIService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    public async Task<AdminDashboardResponse> GetDashboardAsync()
    {
        var totalPatients = await _context.Patients.CountAsync();
        var totalDoctors = await _context.Doctors.CountAsync();
        var totalAppointments = await _context.Appointments.CountAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfDay = today.ToDateTime(TimeOnly.MinValue);
        var endOfDay = today.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var todayAppointments = await _context.Appointments
            .CountAsync(a => a.ScheduledStart >= startOfDay && a.ScheduledStart < endOfDay);
        var activeUsers = await _context.Users.CountAsync(u => u.IsActive);

        return new AdminDashboardResponse
        {
            TotalPatients = totalPatients,
            TotalDoctors = totalDoctors,
            TotalAppointments = totalAppointments,
            TodayAppointments = todayAppointments,
            ActiveUsers = activeUsers,
            Status = "active",
            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<List<UserResponse>> GetUsersAsync()
    {
        var users = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return users.Select(u => new UserResponse
        {
            Id = u.Id,
            Email = u.Email,
            FullName = $"{u.FirstName} {u.LastName}",
            Role = u.Role.ToString(),
            IsActive = u.IsActive,
            LastLoginAt = u.LastLoginAt,
            CreatedAt = u.CreatedAt
        }).ToList();
    }

    public async Task<SystemStatusResponse> GetSystemStatusAsync()
    {
        var databaseConnected = await _context.Database.CanConnectAsync();
        var activeUsers = await _context.Users.CountAsync(u => u.IsActive);

        var aiAvailable = false;
        try
        {
            aiAvailable = await _aiService.HealthCheckAsync();
        }
        catch
        {
            aiAvailable = false;
        }

        return new SystemStatusResponse
        {
            Status = databaseConnected ? "operational" : "degraded",
            DatabaseConnected = databaseConnected,
            AIAvailable = aiAvailable,
            ActiveConnections = activeUsers,
            LastChecked = DateTime.UtcNow
        };
    }
}
