using System.ComponentModel.DataAnnotations;

namespace choir_music_system.Models;

public class MassMusicSheet
{
    public int Id { get; set; }

    public int MassId { get; set; }

    public Mass Mass { get; set; } = null!;

    public int MusicSheetId { get; set; }

    public MusicSheet MusicSheet { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string MassPart { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
}