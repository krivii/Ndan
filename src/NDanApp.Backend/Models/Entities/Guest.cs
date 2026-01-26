using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NDanApp.Backend.Models.Entities;

[Table("guests")]
public class Guest
{
    [Key]
    [Column("guest_id")]
    public Guid GuestId { get; set; }

    [Required]
    [Column("event_id")]
    public Guid EventId { get; set; }

    [MaxLength(100)]
    [Column("nickname")]
    public string? Nickname { get; set; }

    [Required]
    [Column("created_utc")]
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    [ForeignKey("EventId")]
    public Event Event { get; set; } = null!;

    public ICollection<Media> MediaItems { get; set; } = new List<Media>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();

}