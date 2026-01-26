using NDanApp.Backend.Models.Entities;

namespace NDanApp.Backend.Repositories;

public interface IMediaRepository : IRepository<Media>
{
    Task<IEnumerable<Media>> GetByEventIdAsync(Guid eventId, int limit = 100);
    Task<IEnumerable<Media>> GetByGuestIdAsync(Guid guestId);
    Task<int> GetMediaCountByEventAsync(Guid eventId);
    Task<int> GetMediaCountByGuestAsync(Guid guestId);
    Task<IEnumerable<Media>> GetByMediaTypeAsync(Guid eventId, MediaType mediaType);
    Task<Media?> GetWithDetailsAsync(Guid mediaId);
}