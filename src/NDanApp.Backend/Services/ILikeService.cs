

using NDanApp.Backend.Models.DTOs;

namespace NDanApp.Backend.Services;

public interface ILikeService
{
    Task<LikeCreated> AddLikeAsync(AddLikeRequest request, CancellationToken ct = default);
    Task<bool> RemoveLikeAsync(Guid mediaId, Guid guestId, CancellationToken ct = default);
    Task<IEnumerable<MediaLikeStatus>> GetLikeStatusesAsync(List<Guid> mediaIds, Guid guestId, CancellationToken ct = default);
    Task<IEnumerable<LikeDetail>> GetMediaLikesAsync(Guid mediaId, CancellationToken ct = default);
}