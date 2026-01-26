using System.ComponentModel.DataAnnotations;

namespace NDanApp.Backend.Models.DTOs;

public class AddLikeRequest
{
    [Required(ErrorMessage = "Media ID is required")]
    public Guid MediaId { get; set; }

    [Required(ErrorMessage = "Guest ID is required")]
    public Guid GuestId { get; set; }
}

public class RemoveLikeRequest
{
    [Required(ErrorMessage = "Media ID is required")]
    public Guid MediaId { get; set; }

    [Required(ErrorMessage = "Guest ID is required")]
    public Guid GuestId { get; set; }
}


public record LikeCreated(
    Guid Id,
    Guid MediaId
);

public record LikeDetail(
    Guid Id,
    string? Guest,
    DateTimeOffset Created
);

public record MediaLikeStatus(
    Guid MediaId,
    int Count,
    bool IsLiked
);