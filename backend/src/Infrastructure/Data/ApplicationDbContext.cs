using System.Reflection;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Family> Families => Set<Family>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Event> Events => Set<Event>();


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Entity<Member>()
            .HasOne(m => m.Father)
            .WithMany()
            .HasForeignKey(m => m.FatherId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Member>()
            .HasOne(m => m.Mother)
            .WithMany()
            .HasForeignKey(m => m.MotherId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(builder);
    }
}
