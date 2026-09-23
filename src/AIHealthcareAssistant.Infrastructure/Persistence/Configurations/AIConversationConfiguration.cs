using AIHealthcareAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIHealthcareAssistant.Infrastructure.Persistence.Configurations;

public class AIConversationConfiguration : IEntityTypeConfiguration<AIConversation>
{
    public void Configure(EntityTypeBuilder<AIConversation> builder)
    {
        builder.ToTable("AIConversations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.StartedAt).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(4000);
        builder.Property(x => x.CreatedBy).HasMaxLength(256);
        builder.Property(x => x.UpdatedBy).HasMaxLength(256);

        builder.HasIndex(x => x.PatientId);
        builder.HasIndex(x => x.AppointmentId)
            .IsUnique()
            .HasFilter("[AppointmentId] IS NOT NULL AND [IsDeleted] = 0");

        builder.HasMany(x => x.Messages)
            .WithOne(x => x.Conversation)
            .HasForeignKey(x => x.AIConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
