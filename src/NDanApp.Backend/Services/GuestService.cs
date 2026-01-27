using NDanApp.Backend.Models.DTOs;
using NDanApp.Backend.Models.Entities;
using NDanApp.Backend.Repositories;

namespace NDanApp.Backend.Services;

public class GuestService : IGuestService
{
    private readonly IGuestRepository _guestRepo;
    private readonly IMediaRepository _mediaRepo;
    private readonly ILikeRepository _likeRepo;

    public GuestService(
        IGuestRepository guestRepo,
        IMediaRepository mediaRepo,
        ILikeRepository likeRepo)
    {
        _guestRepo = guestRepo;
        _mediaRepo = mediaRepo;
        _likeRepo = likeRepo;
    }

public async Task<GuestCreated> CreateGuestAsync(CreateGuestRequest request, CancellationToken ct = default)
{
    var guest = new Guest
    {
        EventId = request.EventId,
        Nickname = request.Nickname,
        Fingerprint = request.Fingerprint // Store fingerprint
    };

    await _guestRepo.AddAsync(guest, ct);

    return new GuestCreated(guest.GuestId, guest.Nickname, guest.EventId);
}

    public async Task<GuestDetail?> GetGuestDetailAsync(Guid guestId, CancellationToken ct = default)
    {
        var guest = await _guestRepo.GetByIdAsync(guestId, ct);
        if (guest == null) return null;

        var mediaCount = await _mediaRepo.GetMediaCountByGuestAsync(guestId, ct);
        var likesGiven = await _likeRepo.GetLikesGivenByGuestAsync(guestId, ct);

        return new GuestDetail(
            guest.GuestId,
            guest.EventId,
            guest.Nickname,
            guest.CreatedUtc,
            mediaCount,
            likesGiven
        );
    }

    public async Task<IEnumerable<GuestListItem>> GetGuestsByEventAsync(Guid eventId, CancellationToken ct = default)
    {
        var guests = await _guestRepo.GetByEventIdAsync(eventId, ct);
        var result = new List<GuestListItem>();

        foreach (var guest in guests)
        {
            var mediaCount = await _mediaRepo.GetMediaCountByGuestAsync(guest.GuestId, ct);
            result.Add(new GuestListItem(guest.GuestId, guest.Nickname, mediaCount));
        }

        return result.OrderByDescending(g => g.MediaCount);
    }

    public async Task<GuestCreated?> FindByFingerprintAsync(Guid eventId, string fingerprint, CancellationToken ct = default)
    {
        var guest = await _guestRepo.GetByEventAndFingerprintAsync(eventId, fingerprint, ct);
        
        if (guest == null) return null;
        
        return new GuestCreated(guest.GuestId, guest.Nickname, guest.EventId);
    }
}