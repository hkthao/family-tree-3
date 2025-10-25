using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class EventMemberConfiguration : IEntityTypeConfiguration<EventMember>
{
    public void Configure(EntityTypeBuilder<EventMember> builder)
    {
        builder.ToTable("event_member");

        builder.HasKey(em => new { em.EventId, em.MemberId });

        builder.Property(em => em.EventId)
            .HasColumnName("event_id");

        builder.Property(em => em.MemberId)
            .HasColumnName("member_id");

        builder.HasOne(em => em.Event)
            .WithMany(e => e.EventMembers)
            .HasForeignKey(em => em.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(em => em.Member)
            .WithMany(m => m.EventMembers)
            .HasForeignKey(em => em.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
