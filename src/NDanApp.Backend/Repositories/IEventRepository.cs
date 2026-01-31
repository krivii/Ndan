using NDanApp.Backend.Models.Entities;
using NDanApp.Backend.Repositories;

public interface IEventRepository : IRepository<Event>
{
    Task<Event?> GetByInviteTokenAsync(string tokenHash, CancellationToken ct = default);
    Task<bool> IsInviteTokenUniqueAsync(string tokenHash, CancellationToken ct = default);
    Task<IEnumerable<Event>> GetActiveEventsAsync(CancellationToken ct = default);

    
}