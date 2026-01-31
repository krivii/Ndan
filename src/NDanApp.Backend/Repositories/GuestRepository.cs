using Microsoft.EntityFrameworkCore;
using NDanApp.Backend.Data;
using NDanApp.Backend.Models.DTOs;
using NDanApp.Backend.Models.Entities;

namespace NDanApp.Backend.Repositories
{
public class GuestRepository : Repository<Guest>, IGuestRepository
    {
        private readonly AppDbContext _context;
    public GuestRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Guest?> GetByIdAsync(Guid guestId, CancellationToken ct = default)
    {
        return await _context.Guests
            .Include(g => g.Event)
            .FirstOrDefaultAsync(g => g.GuestId == guestId, ct);
    }

        public async Task<Guest?> GetByEventAndNicknameAsync(Guid eventId, string nickname, CancellationToken ct = default)
        {
            return await _context.Guests
                .FirstOrDefaultAsync(g => g.EventId == eventId && g.Nickname == nickname);
        }

        public async Task<IEnumerable<Guest>> GetByEventIdAsync(Guid eventId, CancellationToken ct = default)
        {
            return await _context.Guests
                .Where(g => g.EventId == eventId)
                .OrderBy(g => g.CreatedUtc)
                .ToListAsync();
        }
    }
}
