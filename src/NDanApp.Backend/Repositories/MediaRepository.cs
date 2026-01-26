using Microsoft.EntityFrameworkCore;
using NDanApp.Backend.Data;
using NDanApp.Backend.Models.Entities;

namespace NDanApp.Backend.Repositories;

public class MediaRepository : Repository<Media>, IMediaRepository
{
    public MediaRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Media>> GetByEventIdAsync(Guid eventId, int limit = 100)
    {
        return await _context.Media
            .Where(m => m.EventId == eventId)
            .OrderByDescending(m => m.CreatedUtc)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Media>> GetByGuestIdAsync(Guid guestId)
    {
        return await _context.Media
            .Where(m => m.GuestId == guestId)
            .OrderByDescending(m => m.CreatedUtc)
            .ToListAsync();
    }

    public async Task<int> GetMediaCountByEventAsync(Guid eventId)
    {
        return await _context.Media
            .CountAsync(m => m.EventId == eventId);
    }

    public async Task<int> GetMediaCountByGuestAsync(Guid guestId)
    {
        return await _context.Media
            .CountAsync(m => m.GuestId == guestId);
    }

    public async Task<IEnumerable<Media>> GetByMediaTypeAsync(Guid eventId, MediaType mediaType)
    {
        return await _context.Media
            .Where(m => m.EventId == eventId && m.MediaType == mediaType)
            .OrderByDescending(m => m.CreatedUtc)
            .ToListAsync();
    }

    public async Task<Media?> GetWithDetailsAsync(Guid mediaId)
    {
        return await _context.Media
            .FirstOrDefaultAsync(m => m.MediaId == mediaId);
    }
}