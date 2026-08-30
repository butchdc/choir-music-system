using System.ComponentModel.DataAnnotations;

namespace choir_music_system.Models;

public class MassSong
{
    public int Id { get; set; }

    public int MassId { get; set; }

    public Mass Mass { get; set; } = null!;

    public int SongId { get; set; }
    public Song Song { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string MassPart { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
}