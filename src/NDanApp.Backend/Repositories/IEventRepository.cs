using NDanApp.Backend.Models.Entities;
using NDanApp.Backend.Repositories;

public interface IEventRepository : IRepository<Event>
{
    Task<Event?> GetByInviteTokenHashAsync(string tokenHash);
    Task<bool> IsInviteTokenUniqueAsync(string tokenHash);
    Task<IEnumerable<Event>> GetActiveEventsAsync();
    Task<Event?> GetWithStatsAsync(Guid eventId);
}
