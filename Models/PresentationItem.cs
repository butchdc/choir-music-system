using System.ComponentModel.DataAnnotations;

namespace choir_music_system.Models;

public class PresentationItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(100)]
    public string Type { get; set; } = string.Empty;

    [StringLength(10000)]
    public string? PresentationText { get; set; }

    [StringLength(50)]
    public string LayoutType { get; set; } = "Title + Text";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
}