using choir_music_system.Models;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Data;

public class ChoirDbContext : DbContext
{
    public ChoirDbContext(DbContextOptions<ChoirDbContext> options)
        : base(options)
    {
    }

    public DbSet<Song> Songs { get; set; }
    public DbSet<Mass> Masses => Set<Mass>();
    public DbSet<MassSong> MassSongs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
    }
}