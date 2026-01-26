
using NDanApp.Backend.Models.DTOs;
using NDanApp.Backend.Models.Entities;
using NDanApp.Backend.Repositories;

namespace NDanApp.Backend.Services;

public class LikeService : ILikeService
{
    private readonly ILikeRepository _likeRepo;
    private readonly IGuestRepository _guestRepo;

    public LikeService(ILikeRepository likeRepo, IGuestRepository guestRepo)
    {
        _likeRepo = likeRepo;
        _guestRepo = guestRepo;
    }

    public async Task<LikeCreated> AddLikeAsync(AddLikeRequest request, CancellationToken ct = default)
    {
        // Check if already liked
        var existing = await _likeRepo.GetByMediaAndGuestAsync(request.MediaId, request.GuestId, ct);
        if (existing != null)
            return new LikeCreated(existing.LikeId, existing.MediaId);

        var like = new Like
        {
            MediaId = request.MediaId,
            GuestId = request.GuestId
        };

        await _likeRepo.AddAsync(like, ct);

        return new LikeCreated(like.LikeId, like.MediaId);
    }

    public async Task<bool> RemoveLikeAsync(Guid mediaId, Guid guestId, CancellationToken ct = default)
    {
        var like = await _likeRepo.GetByMediaAndGuestAsync(mediaId, guestId, ct);
        if (like == null) return false;

        await _likeRepo.DeleteAsync(like.LikeId, ct);
        return true;
    }

    public async Task<IEnumerable<MediaLikeStatus>> GetLikeStatusesAsync(List<Guid> mediaIds, Guid guestId, CancellationToken ct = default)
    {
        var likeCounts = await _likeRepo.GetLikeCountsForMediaAsync(mediaIds, ct);
        var result = new List<MediaLikeStatus>();

        foreach (var mediaId in mediaIds)
        {
            var isLiked = await _likeRepo.HasUserLikedAsync(mediaId, guestId, ct);
            result.Add(new MediaLikeStatus(
                mediaId,
                likeCounts.GetValueOrDefault(mediaId, 0),
                isLiked
            ));
        }

        return result;
    }

    public async Task<IEnumerable<LikeDetail>> GetMediaLikesAsync(Guid mediaId, CancellationToken ct = default)
    {
        var likes = await _likeRepo.GetLikesByMediaAsync(mediaId, ct);
        var result = new List<LikeDetail>();

        foreach (var like in likes)
        {
            var guest = await _guestRepo.GetByIdAsync(like.GuestId, ct);
            result.Add(new LikeDetail(
                like.LikeId,
                guest?.Nickname,
                like.CreatedUtc
            ));
        }

        return result;
    }
}