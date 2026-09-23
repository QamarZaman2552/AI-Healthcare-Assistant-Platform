using AIHealthcareAssistant.Application.Common.Interfaces;
using AIHealthcareAssistant.Domain.Common;
using AIHealthcareAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AIHealthcareAssistant.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    private static readonly Type[] SoftDeletableTypes =
    {
        typeof(User),
        typeof(Patient),
        typeof(PatientProfile),
        typeof(Doctor),
        typeof(Specialty),
        typeof(Appointment),
        typeof(PatientIntake),
        typeof(AIConversation),
        typeof(AIConversationMessage),
        typeof(Notification),
        typeof(AdminUser),
        typeof(DoctorAvailability)
    };

    private readonly ICurrentUserService? _currentUserService;
    private readonly string? _actor;
    private readonly bool _hasActor;

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : this(options, null)
    {
    }

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService? currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
        _actor = currentUserService?.Email ?? currentUserService?.UserId?.ToString();
        _hasActor = !string.IsNullOrWhiteSpace(_actor);
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientProfile> PatientProfiles => Set<PatientProfile>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<DoctorSpecialty> DoctorSpecialties => Set<DoctorSpecialty>();
    public DbSet<DoctorAvailability> DoctorAvailabilities => Set<DoctorAvailability>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentStatus> AppointmentStatuses => Set<AppointmentStatus>();
    public DbSet<PatientIntake> PatientIntakes => Set<PatientIntake>();
    public DbSet<AIConversation> AIConversations => Set<AIConversation>();
    public DbSet<AIConversationMessage> AIConversationMessages => Set<AIConversationMessage>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    public override int SaveChanges()
    {
        UpdateAuditEntities();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditEntities()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    if (_hasActor && entry.Entity is AuditableEntity added)
                        added.CreatedBy = _actor;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    if (_hasActor && entry.Entity is AuditableEntity modified)
                        modified.UpdatedBy = _actor;
                    break;

                case EntityState.Deleted:
                    if (SoftDeletableTypes.Contains(entry.Entity.GetType()))
                    {
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.UpdatedAt = now;
                        if (_hasActor && entry.Entity is AuditableEntity softDeleted)
                            softDeleted.UpdatedBy = _actor;
                    }
                    break;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!SoftDeletableTypes.Contains(entityType.ClrType))
                continue;

            modelBuilder.Entity(entityType.ClrType)
                .HasQueryFilter(BuildSoftDeleteFilter(entityType.ClrType));
        }
    }

    private static LambdaExpression BuildSoftDeleteFilter(Type clrType)
    {
        var parameter = Expression.Parameter(clrType, "e");
        var body = Expression.Equal(
            Expression.Property(parameter, nameof(BaseEntity.IsDeleted)),
            Expression.Constant(false));

        return Expression.Lambda(body, parameter);
    }
}
