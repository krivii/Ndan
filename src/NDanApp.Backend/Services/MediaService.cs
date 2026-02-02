
using NDanApp.Backend.Models.DTOs;
using NDanApp.Backend.Models.Entities;
using NDanApp.Backend.Repositories;

namespace NDanApp.Backend.Services;

public class MediaService : IMediaService
{
    private readonly IMediaRepository _mediaRepo;
    private readonly ILikeRepository _likeRepo;
    private readonly IGuestRepository _guestRepo;


    public MediaService(
        IMediaRepository mediaRepo,
        ILikeRepository likeRepo,
        IGuestRepository guestRepo)
    {
        _mediaRepo = mediaRepo;
        _likeRepo = likeRepo;
        _guestRepo = guestRepo;
    }

    

     public async Task<IEnumerable<MediaListItem>> GetMediaByEventAsync(Guid eventId, Guid? currentGuestId = null, CancellationToken ct = default)
    {
        var mediaItems = await _mediaRepo.GetByEventIdAsync(eventId, 100, ct);
        var mediaIds = mediaItems.Select(m => m.MediaId);
        var likeCounts = await _likeRepo.GetLikeCountsForMediaAsync(mediaIds, ct);

        return mediaItems.Select(m => new MediaListItem(
            m.MediaId,
            m.ThumbnailKey ?? m.StorageKey,
            likeCounts.GetValueOrDefault(m.MediaId, 0)
        ));
    }

    public async Task<MediaDetail?> GetMediaDetailAsync(Guid mediaId, Guid? currentGuestId = null, CancellationToken ct = default)
    {
        var media = await _mediaRepo.GetByIdAsync(mediaId, ct);
        if (media == null) return null;

        var likeCount = await _likeRepo.GetLikeCountByMediaAsync(mediaId, ct);
        var isLiked = currentGuestId.HasValue && 
                      await _likeRepo.HasUserLikedAsync(mediaId, currentGuestId.Value, ct);

        string? guestName = null;
        if (media.GuestId.HasValue)
        {
            var guest = await _guestRepo.GetByIdAsync(media.GuestId.Value, ct);
            guestName = guest?.Nickname;
        }

        return new MediaDetail(
            media.MediaId,
            media.MediaType,
            media.StorageKey,
            media.ThumbnailKey,
            guestName,
            media.CreatedUtc,
            likeCount,
            isLiked
        );
    }

    public async Task<MediaCreated> SaveMediaMetadataAsync(SaveMediaMetadataRequest request, CancellationToken ct = default)
    {
        // Just save metadata - file already in Supabase!
        var mediaType = request.MediaType;

        var media = new Media
        {
            EventId = request.EventId,
            GuestId = request.GuestId,
            MediaType = mediaType,
            StorageKey = request.StorageKey,
            MimeType = request.MimeType,
            FileSizeBytes = request.FileSizeBytes,
            ProcessingStatus = ProcessingStatus.Ready
        };

        await _mediaRepo.AddAsync(media, ct);

        return new MediaCreated(media.MediaId, request.StorageKey, request.FileUrl, request.MediaType);
    }

    // Delete needs to call Supabase - see below
    public async Task<bool> DeleteMediaAsync(Guid mediaId, CancellationToken ct = default)
    {
        var media = await _mediaRepo.GetByIdAsync(mediaId, ct);
        if (media == null) return false;

        // Option 1: Frontend handles deletion from Supabase
        // Option 2: Backend calls Supabase API (keep minimal storage service)
        
        await _mediaRepo.DeleteAsync(mediaId, ct);
        return true;
    }

        /// <summary>
    /// Generate unique storage keys and media ID
    /// Frontend will use the storageKey to upload directly to Supabase
    /// </summary>
    public UploadSlotResponse GenerateUploadSlot(UploadSlotRequest request)
    {
        var mediaId = Guid.NewGuid();
        var fileExt = request.FileName?.Split('.').LastOrDefault() ?? "bin";

        var storageKey = $"{mediaId}.{fileExt}";
        var thumbnailKey = $"{mediaId}-thumb.{fileExt}";

        return new UploadSlotResponse
        {
            MediaId = mediaId,
            StorageKey = storageKey,
            ThumbnailKey = thumbnailKey
        };
    }

}