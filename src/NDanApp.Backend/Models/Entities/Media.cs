using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace NDanApp.Backend.Models.Entities;

[Table("media")]
public class Media
{
    [Key]
    [Column("media_id")]
    public Guid MediaId { get; set; }

    [Required]
    [Column("event_id")]
    public Guid EventId { get; set; }

    [Column("guest_id")]
    public Guid? GuestId { get; set; }

    [Required]
    [Column("media_type")]
    public MediaType MediaType { get; set; }

    [Required]
    [MaxLength(512)]
    [Column("storage_key")]
    public string StorageKey { get; set; } = string.Empty;

    [MaxLength(512)]
    [Column("thumbnail_key")]
    public string? ThumbnailKey { get; set; }

    [MaxLength(100)]
    [Column("mime_type")]
    public string? MimeType { get; set; }

    [Column("file_size_bytes")]
    public long? FileSizeBytes { get; set; }

    [Column("duration_seconds")]
    public double? DurationSeconds { get; set; }

    [Required]
    [Column("created_utc")]
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("processing_status")]
    public ProcessingStatus ProcessingStatus { get; set; } = ProcessingStatus.Uploaded;

    [ForeignKey("EventId")]
    public Event Event { get; set; } = null!;

    [ForeignKey("GuestId")]
    public Guest? Guest { get; set; }

    public ICollection<Like> Likes { get; set; } = new List<Like>();

}
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MediaType
{
    [EnumMember(Value = "image")]
    Image = 0,
    
    [EnumMember(Value = "video")]
    Video = 1
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProcessingStatus
{
    [EnumMember(Value = "uploaded")]
    Uploaded = 0,
    
    [EnumMember(Value = "processing")]
    Processing = 1,
    
    [EnumMember(Value = "ready")]
    Ready = 2,
    
    [EnumMember(Value = "failed")]
    Failed = 3
}