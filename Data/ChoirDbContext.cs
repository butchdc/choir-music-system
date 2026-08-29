using choir_music_system.Models;
using Microsoft.EntityFrameworkCore;

namespace choir_music_system.Data;

public class ChoirDbContext : DbContext
{
    public ChoirDbContext(DbContextOptions<ChoirDbContext> options)
        : base(options)
    {
    }

    public DbSet<MusicSheet> MusicSheets => Set<MusicSheet>();
    public DbSet<Mass> Masses => Set<Mass>();
    public DbSet<MassMusicSheet> MassMusicSheets => Set<MassMusicSheet>();
}