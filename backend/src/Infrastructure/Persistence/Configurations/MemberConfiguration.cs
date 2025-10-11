using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations
{
    public class MemberConfiguration : IEntityTypeConfiguration<Member>
    {
        public void Configure(EntityTypeBuilder<Member> builder)
        {
            builder.Property(m => m.FirstName)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(m => m.LastName)
                .HasMaxLength(250)
                .IsRequired();
        }
    }
}
