using NDanApp.Backend.Models.DTOs;
using NDanApp.Backend.Models.Entities;
using NDanApp.Backend.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace NDanApp.Backend.Services;

public class GuestService : IGuestService
{
    private readonly IGuestRepository _guestRepo;
    private readonly IMediaRepository _mediaRepo;
    private readonly ILikeRepository _likeRepo;
    private readonly IEventRepository _eventRepo;

    public GuestService(
        IGuestRepository guestRepo,
        IMediaRepository mediaRepo,
        ILikeRepository likeRepo,
        IEventRepository eventRepo)
    {
        _guestRepo = guestRepo;
        _mediaRepo = mediaRepo;
        _likeRepo = likeRepo;
        _eventRepo = eventRepo;
    }

    public string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLower();
    }


    public async Task<GuestCreated> CreateGuestAsync(CreateGuestRequest request, CancellationToken ct = default)
        {
             
            //zaheshiraj token
            var tokenHash = HashToken(request.EventToken);
            var eventEntity = await _eventRepo.GetByInviteTokenAsync(tokenHash);

            if (eventEntity == null || !eventEntity.IsActive)
                throw new Exception("Invalid or inactive event");

            // 2️⃣ Create guest
            var guest = new Guest
            {
                EventId = eventEntity.EventId,
                Nickname = request.Nickname.Trim()
            };

            await _guestRepo.AddAsync(guest);

            return new GuestCreated(guest.GuestId, guest.Nickname, guest.EventId, request.EventToken);
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

}