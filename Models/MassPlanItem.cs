namespace choir_music_system.Models;

public class MassPlanItem
{
    public int Id { get; set; }

    public int MassId { get; set; }
    public Mass Mass { get; set; } = null!;

    public string ItemType { get; set; } = string.Empty;
    // "Song" or "Presentation"

    public int? SongId { get; set; }
    public Song? Song { get; set; }

    public int? PresentationItemId { get; set; }
    public PresentationItem? PresentationItem { get; set; }

    public string? MassPart { get; set; }

    public int DisplayOrder { get; set; }
}