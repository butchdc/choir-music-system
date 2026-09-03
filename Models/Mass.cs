using System.ComponentModel.DataAnnotations;

namespace choir_music_system.Models;

public class Mass
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public DateTime MassDate { get; set; }
    [StringLength(200)]
    public string? Venue { get; set; }

    [StringLength(4000)]
    public string? MassIntroduction { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

    public ICollection<MassSong> Songs { get; set; }
        = new List<MassSong>();

    [StringLength(500)]
    public string? PresentationBackgroundPath { get; set; }
    [StringLength(500)]
    public string? FinalPresentationFileName { get; set; }

    [StringLength(500)]
    public string? FinalPresentationPath { get; set; }

    public DateTime? FinalPresentationUpdatedDate { get; set; }
    public ICollection<MassPlanItem> PlanItems { get; set; }
    = new List<MassPlanItem>();
}