using GlowUp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GlowUp.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<ProfileSection> ProfileSections => Set<ProfileSection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.HasMany(p => p.Sections)
                  .WithOne(s => s.Profile)
                  .HasForeignKey(s => s.ProfileId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProfileSection>(entity =>
        {
            entity.HasKey(s => s.Id);

            // Persist the enum as a readable string rather than an int.
            entity.Property(s => s.SectionType)
                  .HasConversion<string>()
                  .HasMaxLength(50);

            // One section per type per profile (MVP: a single section per category).
            entity.HasIndex(s => new { s.ProfileId, s.SectionType }).IsUnique();
        });
    }
}
