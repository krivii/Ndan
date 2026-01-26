using Microsoft.EntityFrameworkCore;
using NDanApp.Backend.Data;
using NDanApp.Backend.Models.Entities;

namespace NDanApp.Backend.Repositories;
public class EventRepository : Repository<Event>, IEventRepository
{
    public EventRepository(AppDbContext context) : base(context) { }

    public async Task<Event?> GetByInviteTokenHashAsync(string tokenHash)
    {
        return await _context.Events
            .FirstOrDefaultAsync(e => e.InviteTokenHash == tokenHash);
    }

    public async Task<bool> IsInviteTokenUniqueAsync(string tokenHash)
    {
        return !await _context.Events
            .AnyAsync(e => e.InviteTokenHash == tokenHash);
    }

    public async Task<IEnumerable<Event>> GetActiveEventsAsync()
    {
        return await _context.Events
            .Where(e => e.IsActive)
            .OrderByDescending(e => e.CreatedUtc)
            .ToListAsync();
    }

    public async Task<Event?> GetWithStatsAsync(Guid eventId)
    {
        return await _context.Events
            .Where(e => e.EventId == eventId)
            .FirstOrDefaultAsync();
    }
}