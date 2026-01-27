using NDanApp.Backend.Models.DTOs;

namespace NDanApp.Backend.Services;

public interface IGuestService
{
    Task<GuestCreated> CreateGuestAsync(CreateGuestRequest request, CancellationToken ct = default);
    Task<GuestDetail?> GetGuestDetailAsync(Guid guestId, CancellationToken ct = default);
    Task<IEnumerable<GuestListItem>> GetGuestsByEventAsync(Guid eventId, CancellationToken ct = default);
    Task<GuestCreated?> FindByFingerprintAsync(Guid eventId, string fingerprint, CancellationToken ct = default);
}