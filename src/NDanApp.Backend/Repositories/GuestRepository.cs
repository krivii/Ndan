using Microsoft.EntityFrameworkCore;
using NDanApp.Backend.Data;
using NDanApp.Backend.Models.Entities;

namespace NDanApp.Backend.Repositories;

public class GuestRepository : Repository<Guest>, IGuestRepository
{
    public GuestRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Guest>> GetByEventIdAsync(Guid eventId, CancellationToken ct = default)
    {
        return await _context.Guests
            .Where(g => g.EventId == eventId)
            .OrderBy(g => g.CreatedUtc)
            .ToListAsync(ct);
    }

    public async Task<Guest?> GetByEventAndNicknameAsync(Guid eventId, string nickname, CancellationToken ct = default)
    {
        return await _context.Guests
            .FirstOrDefaultAsync(g => g.EventId == eventId && g.Nickname == nickname, ct);
    }

    public async Task<int> GetGuestCountByEventAsync(Guid eventId, CancellationToken ct = default)
    {
        return await _context.Guests
            .CountAsync(g => g.EventId == eventId, ct);
    }
}