using Microsoft.AspNetCore.Mvc;

using NDanApp.Backend.Models.DTOs;
using NDanApp.Backend.Services;

namespace NDanApp.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GuestsController : ControllerBase
{
    private readonly IGuestService _guestService;
    private readonly ILogger<GuestsController> _logger;

    public GuestsController(IGuestService guestService, ILogger<GuestsController> logger)
    {
        _guestService = guestService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new guest for an event
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(GuestCreated), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GuestCreated>> CreateGuest(
        [FromBody] CreateGuestRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Creating guest for event {EventId}", request.EventId);
        
        var result = await _guestService.CreateGuestAsync(request, ct);
        
        return CreatedAtAction(
            nameof(GetGuest), 
            new { id = result.Id }, 
            result);
    }

    /// <summary>
    /// Get guest by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GuestDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GuestDetail>> GetGuest(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Getting guest {GuestId}", id);
        
        var result = await _guestService.GetGuestDetailAsync(id, ct);
        
        if (result == null)
            return NotFound(new { message = $"Guest with ID {id} not found" });
        
        return Ok(result);
    }

    /// <summary>
    /// Get all guests for an event
    /// </summary>
    [HttpGet("event/{eventId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<GuestListItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GuestListItem>>> GetGuestsByEvent(
        Guid eventId, 
        CancellationToken ct)
    {
        _logger.LogInformation("Getting guests for event {EventId}", eventId);
        
        var guests = await _guestService.GetGuestsByEventAsync(eventId, ct);
        
        return Ok(guests);
    }

    /// <summary>
    /// Find guest by fingerprint
    /// </summary>
    [HttpGet("find")]
    [ProducesResponseType(typeof(GuestCreated), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GuestCreated>> FindGuest(
        [FromQuery] Guid eventId,
        [FromQuery] string fingerprint,
        CancellationToken ct)
    {
        _logger.LogInformation("Finding guest for event {EventId} with fingerprint {Fingerprint}", 
            eventId, fingerprint);
        
        var guest = await _guestService.FindByFingerprintAsync(eventId, fingerprint, ct);
        
        if (guest == null)
            return NotFound();
        
        return Ok(guest);
    }
}