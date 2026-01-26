using NDanApp.Backend.Models.Entities;

namespace NDanApp.Backend.Repositories;

public interface ILikeRepository : IRepository<Like>
{
    Task<Like?> GetByMediaAndGuestAsync(Guid mediaId, Guid guestId, CancellationToken ct = default);
    Task<int> GetLikeCountByMediaAsync(Guid mediaId, CancellationToken ct = default);
    Task<Dictionary<Guid, int>> GetLikeCountsForMediaAsync(IEnumerable<Guid> mediaIds, CancellationToken ct = default);
    Task<bool> HasUserLikedAsync(Guid mediaId, Guid guestId, CancellationToken ct = default);
    Task<IEnumerable<Like>> GetLikesByMediaAsync(Guid mediaId, CancellationToken ct = default);
    Task<int> GetLikesGivenByGuestAsync(Guid guestId, CancellationToken ct = default);
}