using Microsoft.AspNetCore.Mvc;
using NDanApp.Backend.Models.DTOs;
using NDanApp.Backend.Services;

namespace NDanApp.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(IEventService eventService, ILogger<EventsController> logger)
    {
        _eventService = eventService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new event
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EventCreated), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventCreated>> CreateEvent(
        [FromBody] CreateEventRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Creating new event: {EventName}", request.Name);
        
        var result = await _eventService.CreateEventAsync(request, ct);
        
        return CreatedAtAction(
            nameof(GetEvent), 
            new { id = result.Id }, 
            result);
    }

    private object GetEvent()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Validate an invite token and get event access
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(EventAccess), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventAccess>> ValidateInvite(
        [FromBody] ValidateInviteRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Validating invite token");
        
        var result = await _eventService.ValidateInviteAsync(request.InviteToken, ct);
        
        if (result == null)
            return Unauthorized(new { message = "Invalid or expired invite token" });
        
        return Ok(result);
    }

   /// <summary>
    /// Get all events
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventListItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EventListItem>>> GetAllEvents(CancellationToken ct)
    {
        _logger.LogInformation("Getting all events");
        
        var events = await _eventService.GetAllEventsAsync(ct);
        
        return Ok(events);
    }

    /// <summary>
    /// Get event by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EventDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDetail>> GetEvent(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Getting event {EventId}", id);
        
        var result = await _eventService.GetEventDetailAsync(id, ct);
        
        if (result == null)
            return NotFound(new { message = $"Event with ID {id} not found" });
        
        return Ok(result);
    }

    /// <summary>
    /// Deactivate an event
    /// </summary>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateEvent(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Deactivating event {EventId}", id);
        
        var success = await _eventService.DeactivateEventAsync(id, ct);
        
        if (!success)
            return NotFound(new { message = $"Event with ID {id} not found" });
        
        return Ok(new { message = "Event deactivated successfully" });
    }
}