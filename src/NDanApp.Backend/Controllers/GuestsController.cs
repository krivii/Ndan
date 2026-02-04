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
    public async Task<IActionResult> CreateGuest([FromBody] CreateGuestRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _logger.LogInformation("Creating guest for event {EventToken}", request.EventToken);

        try
        {
            var result = await _guestService.CreateGuestAsync(request, ct);


            return Ok(new CreateGuestResponse
            {
                GuestId = result.GuestId,
                EventToken = request.EventToken,
                EventId = result.EventId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
}