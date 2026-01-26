using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace NDanApp.Backend.Models.DTOs;

public class CreateEventRequest
{
    [Required(ErrorMessage = "Event name is required")]
    [MaxLength(255, ErrorMessage = "Event name cannot exceed 255 characters")]
    public string Name { get; set; } = string.Empty;

    public DateTime? StartDateUtc { get; set; }

    public DateTime? EndDateUtc { get; set; }
}

public class ValidateInviteRequest
{
    [Required(ErrorMessage = "Invite token is required")]
    [MinLength(8, ErrorMessage = "Invalid invite token")]
    public string InviteToken { get; set; } = string.Empty;
}

public class UpdateEventRequest
{
    [MaxLength(255)]
    public string? Name { get; set; }

    public DateTime? StartDateUtc { get; set; }

    public DateTime? EndDateUtc { get; set; }

    public bool? IsActive { get; set; }
}


public record EventCreated(
    Guid Id,
    string Name,
    string InviteToken  // Only returned on creation!
);

// For validating invite (minimal - just confirm access)
public record EventAccess(
    Guid Id,
    string Name,
    bool IsActive
);

// For event list (if host has multiple events)
public record EventListItem(
    Guid Id,
    string Name,
    DateTimeOffset? StartDate,
    bool IsActive,
    int MediaCount,
    int GuestCount
);

// For event detail/dashboard
public record EventDetail(
    Guid Id,
    string Name,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    bool IsActive,
    DateTimeOffset Created,
    int MediaCount,
    int GuestCount,
    int TotalLikes
);

