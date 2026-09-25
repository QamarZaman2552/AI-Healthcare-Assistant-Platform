using System.Text;
using AIHealthcareAssistant.Application.Features.Admin;
using AIHealthcareAssistant.Application.Common.Interfaces;
using AIHealthcareAssistant.Application.Features.Appointments;
using AIHealthcareAssistant.Application.Features.Auth;
using AIHealthcareAssistant.Application.Features.Availability;
using AIHealthcareAssistant.Application.Features.Doctors;
using AIHealthcareAssistant.Application.Features.PatientIntakes;
using AIHealthcareAssistant.Application.Features.Patients;
using AIHealthcareAssistant.Application.Features.Specialties;
using AIHealthcareAssistant.Infrastructure.Persistence;
using AIHealthcareAssistant.Infrastructure.Services;
using AIHealthcareAssistant.Infrastructure.Services.Ai;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;


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

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<IAvailabilityService, AvailabilityService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<ISpecialtyService, SpecialtyService>();
        services.AddScoped<IPatientIntakeService, PatientIntakeService>();
        services.AddScoped<IAdminService, AdminService>();

        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                "JwtSettings configuration section is missing.");

        if (string.IsNullOrWhiteSpace(jwtSettings.Key) || jwtSettings.Key.Length < 32)
        {
            throw new InvalidOperationException(
                "JwtSettings:Key is missing or shorter than 32 characters. " +
                "Set it with: dotnet user-secrets set \"JwtSettings:Key\" \"<random string, 64+ chars>\" " +
                "--project src/AIHealthcareAssistant.API " +
                "or via environment variable JwtSettings__Key. See LOCAL-SECRETS.txt.");
        }

        var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

           
        });

        services.AddAuthorization();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", builder =>
            {
                builder.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        services.Configure<AISettings>(
    configuration.GetSection("AISettings"));

        services.AddHttpClient<IAIService, AIService>(
            (provider, client) =>
            {
                var settings =
                    provider
                        .GetRequiredService<IOptions<AISettings>>()
                        .Value;

                client.BaseAddress =
                    new Uri(settings.BaseUrl);

                client.Timeout =
                    TimeSpan.FromSeconds(
                        settings.TimeoutSeconds);
            });

        return services;
    }
}