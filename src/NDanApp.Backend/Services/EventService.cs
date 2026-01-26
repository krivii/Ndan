using System.Security.Cryptography;
using System.Text;
using NDanApp.Backend.Models.DTOs;
using NDanApp.Backend.Models.Entities;
using NDanApp.Backend.Repositories;

namespace NDanApp.Backend.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepo;
    private readonly IMediaRepository _mediaRepo;
    private readonly IGuestRepository _guestRepo;
    private readonly ILikeRepository _likeRepo;

    public EventService(
        IEventRepository eventRepo,
        IMediaRepository mediaRepo,
        IGuestRepository guestRepo,
        ILikeRepository likeRepo)
    {
        _eventRepo = eventRepo;
        _mediaRepo = mediaRepo;
        _guestRepo = guestRepo;
        _likeRepo = likeRepo;
    }

    public async Task<EventCreated> CreateEventAsync(CreateEventRequest request, CancellationToken ct = default)
    {
        var inviteToken = GenerateInviteToken();
        var tokenHash = HashToken(inviteToken);

        var evt = new Event
        {
            Name = request.Name,
            InviteTokenHash = tokenHash,
            StartDateUtc = request.StartDateUtc,
            EndDateUtc = request.EndDateUtc
        };

        await _eventRepo.AddAsync(evt, ct);

        return new EventCreated(evt.EventId, evt.Name, inviteToken);
    }

    public async Task<EventAccess?> ValidateInviteAsync(string inviteToken, CancellationToken ct = default)
    {
        var tokenHash = HashToken(inviteToken);
        var evt = await _eventRepo.GetByInviteTokenHashAsync(tokenHash, ct);

        if (evt == null || !evt.IsActive)
            return null;

        return new EventAccess(evt.EventId, evt.Name, evt.IsActive);
    }

    public async Task<EventDetail?> GetEventDetailAsync(Guid eventId, CancellationToken ct = default)
    {
        var evt = await _eventRepo.GetByIdAsync(eventId, ct);
        if (evt == null) return null;

        var mediaCount = await _mediaRepo.GetMediaCountByEventAsync(eventId, ct);
        var guestCount = await _guestRepo.GetGuestCountByEventAsync(eventId, ct);

        // Get total likes for this event
        var mediaItems = await _mediaRepo.GetByEventIdAsync(eventId, int.MaxValue, ct);
        var mediaIds = mediaItems.Select(m => m.MediaId);
        var likeCounts = await _likeRepo.GetLikeCountsForMediaAsync(mediaIds, ct);
        var totalLikes = likeCounts.Values.Sum();

        return new EventDetail(
            evt.EventId,
            evt.Name,
            evt.StartDateUtc,
            evt.EndDateUtc,
            evt.IsActive,
            evt.CreatedUtc,
            mediaCount,
            guestCount,
            totalLikes
        );
    }

    public async Task<IEnumerable<EventListItem>> GetAllEventsAsync(CancellationToken ct = default)
    {
        var events = await _eventRepo.GetAllAsync(ct);
        var result = new List<EventListItem>();

        foreach (var evt in events)
        {
            var mediaCount = await _mediaRepo.GetMediaCountByEventAsync(evt.EventId, ct);
            var guestCount = await _guestRepo.GetGuestCountByEventAsync(evt.EventId, ct);

            result.Add(new EventListItem(
                evt.EventId,
                evt.Name,
                evt.StartDateUtc,
                evt.IsActive,
                mediaCount,
                guestCount
            ));
        }

        return result.OrderByDescending(e => e.StartDate ?? DateTime.MinValue);
    }    

    public async Task<bool> DeactivateEventAsync(Guid eventId, CancellationToken ct = default)
    {
        var evt = await _eventRepo.GetByIdAsync(eventId, ct);
        if (evt == null) return false;

        evt.IsActive = false;
        await _eventRepo.UpdateAsync(evt, ct);
        return true;
    }

private string GenerateInviteToken()
{
    // Generate cryptographically secure random bytes
    var randomBytes = new byte[9]; // 9 bytes = 12 chars in base64
    RandomNumberGenerator.Fill(randomBytes);
    
    // Convert to base64 and clean up
    var token = Convert.ToBase64String(randomBytes)
        .Replace("+", "")
        .Replace("/", "")
        .Replace("=", "")
        .ToUpper();
    
    return token[..12]; // Take first 12 characters
}

    private string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLower();
    }
}