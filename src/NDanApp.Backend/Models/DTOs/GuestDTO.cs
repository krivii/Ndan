using System.ComponentModel.DataAnnotations;

namespace NDanApp.Backend.Models.DTOs
{
    // Request from frontend to create a guest
    public class CreateGuestRequest
    {
        [Required]
        [MaxLength(64)]
        public string EventToken { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        [MinLength(1)]
        public string Nickname { get; set; } = null!;
    }

    public class CreateGuestResponse
    {
        public Guid GuestId { get; set; }
        public string EventToken { get; set; } 
        public Guid EventId { get; set; }
        }

    // Optional: for returning guest info
    public record GuestCreated(
        Guid GuestId,
        string Nickname,
        Guid EventId,
        string EventToken
    );

    public record GuestListItem(
        Guid GuestId,
        string Nickname,
        int MediaCount
    );

    public record GuestDetail(
        Guid GuestId,
        Guid EventId,
        string Nickname,
        DateTimeOffset Joined,
        int MediaCount,
        int LikesGiven
    );
}
