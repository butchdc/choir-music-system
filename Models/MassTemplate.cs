using System.ComponentModel.DataAnnotations;

namespace choir_music_system.Models;

public class MassTemplate
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; }
        = DateTime.UtcNow;

    public DateTime UpdatedDate { get; set; }
        = DateTime.UtcNow;

    public ICollection<MassTemplateItem> Items { get; set; }
        = new List<MassTemplateItem>();
}