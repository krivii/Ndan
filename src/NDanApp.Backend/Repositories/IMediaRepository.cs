using NDanApp.Backend.Models.Entities;

namespace NDanApp.Backend.Repositories;

public interface IMediaRepository : IRepository<Media>
{
    Task<IEnumerable<Media>> GetByEventIdAsync(Guid eventId, int limit = 100, CancellationToken ct = default);
    Task<IEnumerable<Media>> GetByGuestIdAsync(Guid guestId, CancellationToken ct = default);
    Task<int> GetMediaCountByEventAsync(Guid eventId, CancellationToken ct = default);
    Task<int> GetMediaCountByGuestAsync(Guid guestId, CancellationToken ct = default);
    Task<IEnumerable<Media>> GetByMediaTypeAsync(Guid eventId, MediaType mediaType, CancellationToken ct = default);
}