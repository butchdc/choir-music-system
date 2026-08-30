using System.ComponentModel.DataAnnotations;

namespace choir_music_system.Models;

public class Mass
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public DateTime MassDate { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<MassSong> Songs { get; set; }
        = new List<MassSong>();

    [StringLength(500)]
    public string? PresentationBackgroundPath { get; set; }
    public ICollection<MassPlanItem> PlanItems { get; set; }
    = new List<MassPlanItem>();
}