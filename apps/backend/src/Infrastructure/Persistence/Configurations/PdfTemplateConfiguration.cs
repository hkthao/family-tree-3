using backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Infrastructure.Persistence.Configurations;

public class PdfTemplateConfiguration : IEntityTypeConfiguration<PdfTemplate>
{
    public void Configure(EntityTypeBuilder<PdfTemplate> builder)
    {
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(t => t.HtmlContent)
            .IsRequired(); // No max length as HTML can be large

        builder.Property(t => t.CssContent)
            .IsRequired(); // No max length as CSS can be large
            
        builder.Property(t => t.Placeholders)
            .HasMaxLength(2000); // Store as JSON string, can be large but a limit is good

        builder.HasOne(t => t.Family)
            .WithMany() // PdfTemplate does not have a navigation property back to Family for a collection of templates
            .HasForeignKey(t => t.FamilyId)
            .OnDelete(DeleteBehavior.Cascade); // If family is deleted, delete its templates
    }
}
