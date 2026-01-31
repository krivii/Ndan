using Microsoft.EntityFrameworkCore;
using NDanApp.Backend.Data;
using NDanApp.Backend.Models.Entities;

namespace NDanApp.Backend.Repositories;
public class EventRepository : Repository<Event>, IEventRepository
{
    public EventRepository(AppDbContext context) : base(context) { }

    public async Task<Event?> GetByInviteTokenAsync(string token, CancellationToken ct = default)
    {
        return await _context.Events
            .FirstOrDefaultAsync(e => e.InviteTokenHash == token, ct);
    }

    public async Task<bool> IsInviteTokenUniqueAsync(string token, CancellationToken ct = default)
    {
        return !await _context.Events
            .AnyAsync(e => e.InviteTokenHash == token, ct);
    }

    public async Task<IEnumerable<Event>> GetActiveEventsAsync(CancellationToken ct = default)
    {
        return await _context.Events
            .Where(e => e.IsActive)
            .OrderByDescending(e => e.CreatedUtc)
            .ToListAsync(ct);
    }
}