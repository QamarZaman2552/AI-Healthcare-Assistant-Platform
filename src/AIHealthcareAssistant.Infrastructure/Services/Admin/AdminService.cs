using AIHealthcareAssistant.Application.Common.Interfaces;
using AIHealthcareAssistant.Application.Features.Admin;
using Microsoft.EntityFrameworkCore;
using AIHealthcareAssistant.Domain.Entities;
using AIHealthcareAssistant.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AIHealthcareAssistant.Infrastructure.Services.Admin;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;
    private readonly IAIService _aiService;

    public AdminService(AppDbContext context, IAIService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    public async Task<AdminDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var totalPatients = await _context.Patients.AsNoTracking().CountAsync(cancellationToken);

        var totalDoctors = await _context.Doctors.AsNoTracking().CountAsync(cancellationToken);

        var totalAppointments = await _context.Appointments.AsNoTracking().CountAsync(cancellationToken);

        var todaysAppointments = await _context.Appointments.AsNoTracking()
            .CountAsync(appointment => appointment.ScheduledStart >= today && appointment.ScheduledStart < tomorrow, cancellationToken);

        return new AdminDashboardResponse
        {
            TotalPatients = totalPatients,
            TotalDoctors = totalDoctors,
            TotalAppointments = totalAppointments,
            TodaysAppointments = todaysAppointments
        };
    }

    public async Task<List<AdminUserResponse>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(user => new AdminUserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Roles = user.Role.ToString(),
                IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminSystemStatusResponse> GetSystemStatusAsync(CancellationToken cancellationToken = default)
    {
        bool databaseConnected;

        try
        {
            databaseConnected = await _context.Database.CanConnectAsync(cancellationToken);
        }
        catch
        {
            databaseConnected = false;
        }

        var aiServiceHealthy = await _aiService.HealthCheckAsync(cancellationToken);

        var status = databaseConnected && aiServiceHealthy
            ? "Healthy"
            : "Degraded";

        return new AdminSystemStatusResponse
        {
            Status = status,
            DatabaseConnected = databaseConnected,
            AiServiceHealthy = aiServiceHealthy,
            CheckedAtUtc = DateTime.UtcNow
        };
    }
}