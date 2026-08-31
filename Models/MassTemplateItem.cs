namespace choir_music_system.Models;

public class MassTemplateItem
{
    public int Id { get; set; }

    public int MassTemplateId { get; set; }

    public MassTemplate MassTemplate { get; set; } = null!;

    public string ItemType { get; set; } = string.Empty;

    public int? SongId { get; set; }

    public Song? Song { get; set; }

    public int? PresentationItemId { get; set; }

    public PresentationItem? PresentationItem { get; set; }

    public string? MassPart { get; set; }

    public int DisplayOrder { get; set; }
}