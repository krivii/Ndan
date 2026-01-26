using NDanApp.Backend.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace NDanApp.Backend.Models.DTOs;

public class UploadMediaRequest
{
    [Required]
    public Guid EventId { get; set; }

    public Guid? GuestId { get; set; }

    [Required]
    public IFormFile File { get; set; } = null!;
}

public record MediaItemResponse(
    Guid MediaId,
    MediaType MediaType,
    string? ThumbnailUrl,
    int LikeCount
);

public record MediaDetailResponse(
    Guid MediaId,
    MediaType MediaType,
    string Url,
    string? ThumbnailUrl,
    string? GuestName,
    DateTimeOffset CreatedUtc,
    int LikeCount,
    bool IsLikedByMe,
    long? FileSizeBytes,
    double? DurationSeconds
);

public record MediaUploadResponse(
    Guid MediaId,
    string Url
);