using Microsoft.EntityFrameworkCore;
using NDanApp.Backend.Data;
using NDanApp.Backend.Models.Entities;

namespace NDanApp.Backend.Repositories;

public class LikeRepository : Repository<Like>, ILikeRepository
{
    public LikeRepository(AppDbContext context) : base(context) { }

    public async Task<Like?> GetByMediaAndGuestAsync(Guid mediaId, Guid guestId, CancellationToken ct = default)
    {
        return await _context.Likes
            .FirstOrDefaultAsync(l => l.MediaId == mediaId && l.GuestId == guestId, ct);
    }

    public async Task<int> GetLikeCountByMediaAsync(Guid mediaId, CancellationToken ct = default)
    {
        return await _context.Likes
            .CountAsync(l => l.MediaId == mediaId, ct);
    }

    public async Task<Dictionary<Guid, int>> GetLikeCountsForMediaAsync(IEnumerable<Guid> mediaIds, CancellationToken ct = default)
    {
        return await _context.Likes
            .Where(l => mediaIds.Contains(l.MediaId))
            .GroupBy(l => l.MediaId)
            .Select(g => new { MediaId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.MediaId, x => x.Count, ct);
    }

    public async Task<bool> HasUserLikedAsync(Guid mediaId, Guid guestId, CancellationToken ct = default)
    {
        return await _context.Likes
            .AnyAsync(l => l.MediaId == mediaId && l.GuestId == guestId, ct);
    }

    public async Task<IEnumerable<Like>> GetLikesByMediaAsync(Guid mediaId, CancellationToken ct = default)
    {
        return await _context.Likes
            .Where(l => l.MediaId == mediaId)
            .OrderByDescending(l => l.CreatedUtc)
            .ToListAsync(ct);
    }

    public async Task<int> GetLikesGivenByGuestAsync(Guid guestId, CancellationToken ct = default)
    {
        return await _context.Likes
            .CountAsync(l => l.GuestId == guestId, ct);
    }
}