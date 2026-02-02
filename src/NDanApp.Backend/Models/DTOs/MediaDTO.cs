using NDanApp.Backend.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace NDanApp.Backend.Models.DTOs;

public class UploadMediaRequest
{
    [Required(ErrorMessage = "Event ID is required")]
    public Guid EventId { get; set; }

    public Guid? GuestId { get; set; }

    [Required(ErrorMessage = "File is required")]
    public IFormFile File { get; set; } = null!;
}

public class BulkUploadMediaRequest
{
    [Required(ErrorMessage = "Event ID is required")]
    public Guid EventId { get; set; }

    public Guid? GuestId { get; set; }

    [Required(ErrorMessage = "At least one file is required")]
    [MinLength(1, ErrorMessage = "At least one file is required")]
    [MaxLength(50, ErrorMessage = "Cannot upload more than 50 files at once")]
    public List<IFormFile> Files { get; set; } = new();
}

public class GetMediaRequest
{
    public Guid? EventId { get; set; }
    public Guid? GuestId { get; set; }
    public string? MediaType { get; set; } // "image" or "video"
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
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

// For gallery/list
public record MediaListItem(
    Guid Id,
    string Key,        // Storage key only
    int Likes
);

// For full view
public record MediaDetail(
    Guid Id,
    MediaType Type,
    string Key,
    string? ThumbKey,
    string? Guest,
    DateTimeOffset Created,
    int Likes,
    bool Liked
);

// For upload
public record MediaCreated(
    Guid Id,
    string Key,
    string fileUrl,
    MediaType type);

// No more IFormFile - just metadata!
public class SaveMediaMetadataRequest
{
    [Required]
    public Guid EventId { get; set; }

    public Guid? GuestId { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(512)]
    public string StorageKey { get; set; } = string.Empty;

    [Required]
    public string FileUrl { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? MimeType { get; set; }

    public long? FileSizeBytes { get; set; }

    [Required]
    public MediaType MediaType{ get; set; }  
}

public class UploadSlotRequest
{
    public Guid GuestId { get; set; }
    public string? FileName { get; set; }
}

public class UploadSlotResponse
{
    public Guid MediaId { get; set; }
    public string StorageKey { get; set; } = null!;
    public string ThumbnailKey { get; set; } = null!;
}

