using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class EventMemberConfiguration : IEntityTypeConfiguration<EventMember>
{
    public void Configure(EntityTypeBuilder<EventMember> builder)
    {
        builder.HasKey(em => new { em.EventId, em.MemberId });

        builder.HasOne(em => em.Event)
            .WithMany(e => e.EventMembers)
            .HasForeignKey(em => em.EventId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(em => em.Member)
            .WithMany(m => m.EventMembers)
            .HasForeignKey(em => em.MemberId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
