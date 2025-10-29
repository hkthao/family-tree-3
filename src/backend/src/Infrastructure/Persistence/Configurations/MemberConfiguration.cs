using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("member");

        builder.Ignore(m => m.FullName); // Keep this as it was explicitly ignored

        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.Created).HasColumnName("created");
        builder.Property(m => m.CreatedBy).HasColumnName("created_by");
        builder.Property(m => m.LastModified).HasColumnName("last_modified");
        builder.Property(m => m.LastModifiedBy).HasColumnName("last_modified_by");

        builder.Property(m => m.LastName)
            .HasColumnName("last_name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.FirstName)
            .HasColumnName("first_name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Code)
            .HasColumnName("code")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(m => m.Nickname)
            .HasColumnName("nickname")
            .HasMaxLength(100);

        builder.Property(m => m.Gender)
            .HasColumnName("gender")
            .HasMaxLength(20);

        builder.Property(m => m.DateOfBirth)
            .HasColumnName("date_of_birth");

        builder.Property(m => m.DateOfDeath)
            .HasColumnName("date_of_death");

        builder.Property(m => m.PlaceOfBirth)
            .HasColumnName("place_of_birth")
            .HasMaxLength(200);

        builder.Property(m => m.PlaceOfDeath)
            .HasColumnName("place_of_death")
            .HasMaxLength(200);

        builder.Property(m => m.Occupation)
            .HasColumnName("occupation")
            .HasMaxLength(200);

        builder.Property(m => m.AvatarUrl)
            .HasColumnName("avatar_url")
            .HasMaxLength(500);

        builder.Property(m => m.Biography)
            .HasColumnName("biography")
            .HasMaxLength(4000);

        builder.Property(m => m.FamilyId)
            .HasColumnName("family_id")
            .IsRequired();

        builder.Property(m => m.IsRoot)
            .HasColumnName("is_root");

        builder.HasIndex(m => m.Code)
            .IsUnique();

        builder.HasOne(m => m.Family)
            .WithMany()
            .HasForeignKey(m => m.FamilyId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}