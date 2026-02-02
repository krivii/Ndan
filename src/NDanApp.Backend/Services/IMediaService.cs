using NDanApp.Backend.Models.DTOs;

namespace NDanApp.Backend.Services;

public interface IMediaService
{
    Task<MediaCreated> SaveMediaMetadataAsync(SaveMediaMetadataRequest request, CancellationToken ct = default);
    Task<IEnumerable<MediaListItem>> GetMediaByEventAsync(Guid eventId, Guid? currentGuestId = null, CancellationToken ct = default);
    Task<MediaDetail?> GetMediaDetailAsync(Guid mediaId, Guid? currentGuestId = null, CancellationToken ct = default);
    Task<bool> DeleteMediaAsync(Guid mediaId, CancellationToken ct = default);
    UploadSlotResponse GenerateUploadSlot(UploadSlotRequest request);
    
}