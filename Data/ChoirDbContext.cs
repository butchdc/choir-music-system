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
}