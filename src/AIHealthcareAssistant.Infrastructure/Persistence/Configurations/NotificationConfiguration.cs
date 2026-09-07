using AIHealthcareAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIHealthcareAssistant.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Message).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.Type).HasConversion<int>().IsRequired();
        builder.Property(x => x.IsRead).HasDefaultValue(false);
        builder.Property(x => x.ActionUrl).HasMaxLength(500);

        builder.HasIndex(x => new { x.UserId, x.IsRead });
    }
}
