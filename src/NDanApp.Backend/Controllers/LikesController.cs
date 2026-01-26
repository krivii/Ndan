using Microsoft.AspNetCore.Mvc;
using NDanApp.Backend.Models.DTOs;
using NDanApp.Backend.Services;

namespace NDanApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LikesController : ControllerBase
{
    private readonly ILikeService _likeService;
    private readonly ILogger<LikesController> _logger;

    public LikesController(ILikeService likeService, ILogger<LikesController> logger)
    {
        _likeService = likeService;
        _logger = logger;
    }

    /// <summary>
    /// Add a like to media
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(LikeCreated), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LikeCreated>> AddLike(
        [FromBody] AddLikeRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Adding like to media {MediaId} by guest {GuestId}", 
            request.MediaId, request.GuestId);
        
        var result = await _likeService.AddLikeAsync(request, ct);
        
        return CreatedAtAction(
            nameof(GetMediaLikes), 
            new { mediaId = result.MediaId }, 
            result);
    }

    /// <summary>
    /// Remove a like from media
    /// </summary>
    [HttpDelete("media/{mediaId:guid}/guest/{guestId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveLike(
        Guid mediaId, 
        Guid guestId, 
        CancellationToken ct)
    {
        _logger.LogInformation("Removing like from media {MediaId} by guest {GuestId}", 
            mediaId, guestId);
        
        var success = await _likeService.RemoveLikeAsync(mediaId, guestId, ct);
        
        if (!success)
            return NotFound(new { message = "Like not found" });
        
        return NoContent();
    }

    /// <summary>
    /// Get all likes for a specific media
    /// </summary>
    [HttpGet("media/{mediaId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<LikeDetail>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LikeDetail>>> GetMediaLikes(
        Guid mediaId, 
        CancellationToken ct)
    {
        _logger.LogInformation("Getting likes for media {MediaId}", mediaId);
        
        var likes = await _likeService.GetMediaLikesAsync(mediaId, ct);
        
        return Ok(likes);
    }
 

    /// <summary>
    /// Toggle like (add if not liked, remove if already liked)
    /// </summary>
    [HttpPost("toggle")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> ToggleLike(
        [FromBody] AddLikeRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Toggling like for media {MediaId} by guest {GuestId}", 
            request.MediaId, request.GuestId);
        
        // Check if already liked
        var statuses = await _likeService.GetLikeStatusesAsync(
            new List<Guid> { request.MediaId }, 
            request.GuestId, 
            ct);
        
        var status = statuses.FirstOrDefault();
        
        if (status?.IsLiked == true)
        {
            // Remove like
            await _likeService.RemoveLikeAsync(request.MediaId, request.GuestId, ct);
            return Ok(new { 
                action = "removed", 
                mediaId = request.MediaId,
                newCount = status.Count - 1
            });
        }
        else
        {
            // Add like
            var result = await _likeService.AddLikeAsync(request, ct);
            return Ok(new { 
                action = "added", 
                mediaId = request.MediaId,
                likeId = result.Id
            });
        }
    }
}

