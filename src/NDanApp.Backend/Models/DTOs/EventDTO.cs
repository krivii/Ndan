using System.ComponentModel.DataAnnotations;

namespace NDanApp.Backend.Models.DTOs;

public class CreateEventRequest
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDateUtc { get; set; }
    public DateTime? EndDateUtc { get; set; }
}

public class ValidateInviteRequest
{
    [Required]
    public string InviteToken { get; set; } = string.Empty;
}

public class EventResponse
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDateUtc { get; set; }
    public DateTime? EndDateUtc { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedUtc { get; set; }

}