using AIHealthcareAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIHealthcareAssistant.Infrastructure.Persistence.Configurations;

public class AIConversationMessageConfiguration : IEntityTypeConfiguration<AIConversationMessage>
{
    public void Configure(EntityTypeBuilder<AIConversationMessage> builder)
    {
        builder.ToTable("AIConversationMessages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Role).HasConversion<int>().IsRequired();
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.SequenceNumber).IsRequired();

        builder.HasIndex(x => new { x.AIConversationId, x.SequenceNumber }).IsUnique();
    }
}
