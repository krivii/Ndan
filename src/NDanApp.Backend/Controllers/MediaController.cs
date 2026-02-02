using Microsoft.AspNetCore.Mvc;
using NDanApp.Backend.Models.DTOs;
using NDanApp.Backend.Services;

namespace NDanApp.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;
    private readonly ILogger<MediaController> _logger;

    public MediaController(IMediaService mediaService, ILogger<MediaController> logger)
    {
        _mediaService = mediaService;
        _logger = logger;
    }

    /// <summary>
    /// Save media metadata after frontend upload to Supabase
    /// </summary>
    [HttpPost("metadata")]
    [ProducesResponseType(typeof(MediaCreated), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MediaCreated>> SaveMediaMetadata(
        [FromBody] SaveMediaMetadataRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Saving media metadata for event {EventId}", request.EventId);
        
        var result = await _mediaService.SaveMediaMetadataAsync(request, ct);
        
        return CreatedAtAction(
            nameof(GetMedia), 
            new { id = result.Id }, 
            result);
    }

    /// <summary>
    /// Get media by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MediaDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediaDetail>> GetMedia(
        Guid id, 
        [FromQuery] Guid? guestId,
        CancellationToken ct)
    {
        _logger.LogInformation("Getting media {MediaId}", id);
        
        var result = await _mediaService.GetMediaDetailAsync(id, guestId, ct);
        
        if (result == null)
            return NotFound(new { message = $"Media with ID {id} not found" });
        
        return Ok(result);
    }

    /// <summary>
    /// Get all media for an event
    /// </summary>
    [HttpGet("event/{eventId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<MediaListItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MediaListItem>>> GetMediaByEvent(
        Guid eventId,
        [FromQuery] Guid? guestId,
        CancellationToken ct)
    {
        _logger.LogInformation("Getting media for event {EventId}", eventId);
        
        var media = await _mediaService.GetMediaByEventAsync(eventId, guestId, ct);
        
        return Ok(media);
    }

    /// <summary>
    /// Delete media (removes from DB and Supabase storage)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMedia(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Deleting media {MediaId}", id);
        
        var success = await _mediaService.DeleteMediaAsync(id, ct);
        
        if (!success)
            return NotFound(new { message = $"Media with ID {id} not found" });
        
        return NoContent();
    }

        /// <summary>
    /// Request a new media ID and storage keys for upload
    /// Frontend will upload directly to storage using these keys
    /// </summary>
    [HttpPost("upload-slot")]
    [ProducesResponseType(typeof(UploadSlotResponse), StatusCodes.Status200OK)]
    public ActionResult<UploadSlotResponse> GetUploadSlot([FromBody] UploadSlotRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Generating upload slot for guest {GuestId}", request.GuestId);

        var slot = _mediaService.GenerateUploadSlot(request);
        return Ok(slot);
    }
   
}