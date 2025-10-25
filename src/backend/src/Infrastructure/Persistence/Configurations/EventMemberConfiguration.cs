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

        builder.HasOne<Event>()
            .WithMany(e => e.RelatedMembers)
            .HasForeignKey(em => em.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(em => em.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
