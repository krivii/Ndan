using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NDanApp.Backend.Models.Entities;

[Table("likes")]
public class Like
{
    [Key]
    [Column("like_id")]
    public Guid LikeId { get; set; }

    [Required]
    [Column("media_id")]
    public Guid MediaId { get; set; }

    [Required]
    [Column("guest_id")]
    public Guid GuestId { get; set; }

    [Required]
    [Column("created_utc")]
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        // Navigation properties
    [ForeignKey("MediaId")]
    public Media Media { get; set; } = null!;

    [ForeignKey("GuestId")]
    public Guest Guest { get; set; } = null!;

}