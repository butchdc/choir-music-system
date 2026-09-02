using System.ComponentModel.DataAnnotations;

namespace choir_music_system.Models;

public class AppUser
{
    public int Id { get; set; }

    [Required]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(320)]
    public string NormalizedEmail { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? DisplayName { get; set; }

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "Member";

    public bool IsActive { get; set; } = true;

    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    [MaxLength(320)]
    public string? InvitedBy { get; set; }
}