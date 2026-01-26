using Microsoft.EntityFrameworkCore;
using NDanApp.Backend.Data;
using NDanApp.Backend.Models.Entities;

namespace NDanApp.Backend.Repositories;

public class MediaRepository : Repository<Media>, IMediaRepository
{
    public MediaRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Media>> GetByEventIdAsync(Guid eventId, int limit = 100, CancellationToken ct = default)
    {
        return await _context.Media
            .Where(m => m.EventId == eventId)
            .OrderByDescending(m => m.CreatedUtc)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Media>> GetByGuestIdAsync(Guid guestId, CancellationToken ct = default)
    {
        return await _context.Media
            .Where(m => m.GuestId == guestId)
            .OrderByDescending(m => m.CreatedUtc)
            .ToListAsync(ct);
    }

    public async Task<int> GetMediaCountByEventAsync(Guid eventId, CancellationToken ct = default)
    {
        return await _context.Media
            .CountAsync(m => m.EventId == eventId, ct);
    }

    public async Task<int> GetMediaCountByGuestAsync(Guid guestId, CancellationToken ct = default)
    {
        return await _context.Media
            .CountAsync(m => m.GuestId == guestId, ct);
    }

    public async Task<IEnumerable<Media>> GetByMediaTypeAsync(Guid eventId, MediaType mediaType, CancellationToken ct = default)
    {
        return await _context.Media
            .Where(m => m.EventId == eventId && m.MediaType == mediaType)
            .OrderByDescending(m => m.CreatedUtc)
            .ToListAsync(ct);
    }
}