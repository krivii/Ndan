using System.ComponentModel.DataAnnotations;

namespace NDanApp.Backend.Models.DTOs;

public class CreateGuestRequest
{
    [Required]
    public Guid EventId { get; set; }

    [MaxLength(100)]
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