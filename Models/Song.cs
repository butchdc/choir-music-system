using System.ComponentModel.DataAnnotations;

namespace choir_music_system.Models;

public class Song
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    [StringLength(100)]
    public string? SuggestedMassPart { get; set; }

    public string? Composer { get; set; }

    public string? Arrangement { get; set; }

    public string? Key { get; set; }

    public string PdfFileName { get; set; } = string.Empty;

    public string PdfPath { get; set; } = string.Empty;

    public string? Notes { get; set; }

    [StringLength(100)]
    public string? OneLicenseNumber { get; set; }

    [StringLength(200)]
    public string? Publisher { get; set; }

    [StringLength(500)]
    public string? CopyrightText { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
}