using NDanApp.Backend.Models.Entities;

namespace NDanApp.Backend.Repositories;

public interface IGuestRepository : IRepository<Guest>
{
    Task<IEnumerable<Guest>> GetByEventIdAsync(Guid eventId, CancellationToken ct = default);
    Task<Guest?> GetByEventAndNicknameAsync(Guid eventId, string nickname, CancellationToken ct = default);
    Task<int> GetGuestCountByEventAsync(Guid eventId, CancellationToken ct = default);
    Task<Guest?> GetByEventAndFingerprintAsync(Guid eventId, string fingerprint, CancellationToken ct = default);
}