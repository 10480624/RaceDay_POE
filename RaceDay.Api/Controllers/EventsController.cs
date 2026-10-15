using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public EventsController(RaceDayContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _context.Events
            .AsNoTracking()
            .OrderBy(e => e.EventDate)
            .Select(e => new
            {
                e.EventId,
                e.Name,
                e.Description,
                e.EventDate,
                e.Location,
                e.Distance,
                e.EventType,
                e.OrganiserId
            })
            .ToListAsync();

        return Ok(events);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEvent(int id)
    {
        var eventItem = await _context.Events
            .AsNoTracking()
            .Where(e => e.EventId == id)
            .Select(e => new
            {
                e.EventId,
                e.Name,
                e.Description,
                e.EventDate,
                e.Location,
                e.Distance,
                e.EventType,
                e.OrganiserId
            })
            .SingleOrDefaultAsync();

        if (eventItem == null)
            return NotFound();

        return Ok(eventItem);
    }

    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateEvent(
        [FromBody] CreateEventRequest request)
    {
        var validationError = ValidateEventRequest(
            request.Name,
            request.Location,
            request.EventDate,
            request.Distance,
            request.EventType);

        if (validationError != null)
            return BadRequest(new { message = validationError });

        var organiserId = GetOrganiserId();

        if (organiserId == null)
            return Unauthorized();

        var eventType = request.EventType.Trim().ToLowerInvariant();

        var eventItem = new Event
        {
            OrganiserId = organiserId.Value,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            EventDate = request.EventDate.Date,
            Location = request.Location.Trim(),
            Distance = request.Distance,
            EventType = eventType
        };

        _context.Events.Add(eventItem);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEvent),
            new { id = eventItem.EventId },
            new
            {
                eventItem.EventId,
                eventItem.Name,
                eventItem.Description,
                eventItem.EventDate,
                eventItem.Location,
                eventItem.Distance,
                eventItem.EventType,
                eventItem.OrganiserId
            });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateEvent(
        int id,
        [FromBody] UpdateEventRequest request)
    {
        var validationError = ValidateEventRequest(
            request.Name,
            request.Location,
            request.EventDate,
            request.Distance,
            request.EventType);

        if (validationError != null)
            return BadRequest(new { message = validationError });

        var organiserId = GetOrganiserId();

        if (organiserId == null)
            return Unauthorized();

        var eventItem = await _context.Events
            .SingleOrDefaultAsync(e => e.EventId == id);

        if (eventItem == null)
            return NotFound();

        if (eventItem.OrganiserId != organiserId.Value)
            return Forbid();

        eventItem.Name = request.Name.Trim();
        eventItem.Description = request.Description?.Trim();
        eventItem.EventDate = request.EventDate.Date;
        eventItem.Location = request.Location.Trim();
        eventItem.Distance = request.Distance;
        eventItem.EventType = request.EventType.Trim().ToLowerInvariant();

        await _context.SaveChangesAsync();

        return Ok(new
        {
            eventItem.EventId,
            eventItem.Name,
            eventItem.Description,
            eventItem.EventDate,
            eventItem.Location,
            eventItem.Distance,
            eventItem.EventType,
            eventItem.OrganiserId
        });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var organiserId = GetOrganiserId();

        if (organiserId == null)
            return Unauthorized();

        var eventItem = await _context.Events
            .SingleOrDefaultAsync(e => e.EventId == id);

        if (eventItem == null)
            return NotFound();

        if (eventItem.OrganiserId != organiserId.Value)
            return Forbid();

        var hasEnrolments = await _context.Enrolments
            .AnyAsync(e => e.EventId == id);

        if (hasEnrolments)
        {
            return Conflict(new
            {
                message = "This event cannot be deleted because participants are enrolled."
            });
        }

        _context.Events.Remove(eventItem);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private int? GetOrganiserId()
    {
        var organiserIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (int.TryParse(organiserIdValue, out var organiserId))
            return organiserId;

        return null;
    }

    private static string? ValidateEventRequest(
        string? name,
        string? location,
        DateTime eventDate,
        decimal distance,
        string? eventType)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Event name is required.";

        if (string.IsNullOrWhiteSpace(location))
            return "Event location is required.";

        if (eventDate == default)
            return "A valid event date is required.";

        if (eventDate.Date < DateTime.UtcNow.Date)
            return "Event date cannot be in the past.";

        if (distance <= 0)
            return "Distance must be greater than zero.";

        if (string.IsNullOrWhiteSpace(eventType))
            return "Event type is required.";

        var normalizedType = eventType.Trim().ToLowerInvariant();

        if (normalizedType is not ("run" or "walk" or "cycle"))
            return "Event type must be run, walk, or cycle.";

        return null;
    }
}

public class CreateEventRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public string EventType { get; set; } = string.Empty;
}

public class UpdateEventRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public string EventType { get; set; } = string.Empty;
}
