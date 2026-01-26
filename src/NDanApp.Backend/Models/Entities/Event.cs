using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NDanApp.Backend.Models.Entities;

[Table("events")]
public class Event
{
    [Key]
    [Column("event_id")]
    public Guid EventId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    [Column("invite_token_hash")]
    public string InviteTokenHash { get; set; } = string.Empty;

    [Column("start_date_utc")]
    public DateTime? StartDateUtc { get; set; }

    [Column("end_date_utc")]
    public DateTime? EndDateUtc { get; set; }

    [Required]
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Required]
    [Column("created_utc")]
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Guest> Guests { get; set; } = new List<Guest>();
    public ICollection<Media> MediaItems { get; set; } = new List<Media>();

}