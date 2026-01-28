using System.ComponentModel.DataAnnotations;

namespace NDanApp.Backend.Models.DTOs;

public class CreateGuestRequest
{
    [Required(ErrorMessage = "Event ID is required")]
    public Guid EventId { get; set; }

    [MaxLength(100, ErrorMessage = "Nickname cannot exceed 100 characters")]
    [MinLength(1, ErrorMessage = "Nickname must be at least 1 character")]
    public string? Nickname { get; set; }
    
    [MaxLength(100)]
    public string? Fingerprint { get; set; }
}

public class UpdateGuestRequest
{
    [MaxLength(100, ErrorMessage = "Nickname cannot exceed 100 characters")]
    [MinLength(1, ErrorMessage = "Nickname must be at least 1 character")]
    public string? Nickname { get; set; }
}

public record GuestCreated(
    Guid Id,
    string? Nickname
);

public record GuestListItem(
    Guid Id,
    string? Nickname,
    int MediaCount  // How many photos they uploaded
);

public record GuestDetail(
    Guid Id,
    Guid EventId,
    string? Nickname,
    DateTimeOffset Joined,
    int MediaCount,
    int LikesGiven
);