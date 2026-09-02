using choir_music_system.Models;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Data;

public class ChoirDbContext : DbContext
{

    public DbSet<PresentationItem> PresentationItems { get; set; }
    public ChoirDbContext(DbContextOptions<ChoirDbContext> options)
        : base(options)
    {
    }

    public DbSet<MassPlanItem> MassPlanItems { get; set; }

    public DbSet<Song> Songs { get; set; }
    public DbSet<Mass> Masses => Set<Mass>();
    public DbSet<MassSong> MassSongs { get; set; }

    public DbSet<MassTemplate> MassTemplates { get; set; } = null!;

    public DbSet<MassTemplateItem> MassTemplateItems { get; set; } = null!;

    public DbSet<AppUser> AppUsers => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>()
            .HasIndex(x => x.NormalizedEmail)
            .IsUnique();

        // Keep using the existing database tables after the C# rename.
        modelBuilder.Entity<Song>()
            .ToTable("MusicSheets");

        modelBuilder.Entity<MassSong>()
            .ToTable("MassMusicSheets");

        // Keep using the existing foreign-key column.
        modelBuilder.Entity<MassSong>()
            .Property(x => x.SongId)
            .HasColumnName("MusicSheetId");

        modelBuilder.Entity<MassSong>()
            .HasOne(x => x.Song)
            .WithMany()
            .HasForeignKey(x => x.SongId);

        modelBuilder.Entity<MassPlanItem>()
            .HasOne(x => x.Mass)
            .WithMany(x => x.PlanItems)
            .HasForeignKey(x => x.MassId);

        modelBuilder.Entity<MassPlanItem>()
            .HasOne(x => x.Song)
            .WithMany()
            .HasForeignKey(x => x.SongId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MassPlanItem>()
            .HasOne(x => x.PresentationItem)
            .WithMany()
            .HasForeignKey(x => x.PresentationItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}