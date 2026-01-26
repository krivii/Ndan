using NDanApp.Backend.Models.DTOs;
using NDanApp.Backend.Models.Entities;

namespace NDanApp.Backend.Services;

public interface IEventService
{
    Task<EventCreated> CreateEventAsync(CreateEventRequest request, CancellationToken ct = default);
    Task<EventAccess?> ValidateInviteAsync(string inviteToken, CancellationToken ct = default);
    Task<EventDetail?> GetEventDetailAsync(Guid eventId, CancellationToken ct = default);
    Task<IEnumerable<EventListItem>> GetAllEventsAsync(CancellationToken ct = default);
    Task<bool> DeactivateEventAsync(Guid eventId, CancellationToken ct = default);
}