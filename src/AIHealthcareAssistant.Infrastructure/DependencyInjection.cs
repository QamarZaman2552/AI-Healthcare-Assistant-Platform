using AIHealthcareAssistant.Application.Common.Interfaces;
using AIHealthcareAssistant.Application.Features.Appointments;
using AIHealthcareAssistant.Application.Features.Availability;
using AIHealthcareAssistant.Infrastructure.Persistence;
using AIHealthcareAssistant.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AIHealthcareAssistant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddScoped<IAvailabilityService, AvailabilityService>();
        services.AddScoped<IAppointmentService, AppointmentService>();

        return services;
    }
}
