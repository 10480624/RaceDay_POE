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
                e.BannerImageUrl,
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

    // Organisers can view enrolments for their own events.
    [HttpGet("{id:int}/enrolments")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetEventEnrolments(int id)
    {
        var organiserId = GetOrganiserId();

        if (organiserId == null)
            return Unauthorized();

        var eventItem = await _context.Events
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.EventId == id);

        if (eventItem == null)
            return NotFound();

        if (eventItem.OrganiserId != organiserId.Value)
            return Forbid();

        var enrolments = await _context.Enrolments
            .AsNoTracking()
            .Where(e => e.EventId == id)
            .OrderBy(e => e.EnrolmentDate)
            .Select(e => new
            {
                e.EnrolmentId,
                e.UserId,
                e.EventId,
                e.CategoryId,
                CategoryName = e.Category.CategoryName,
                e.EnrolmentDate,
                e.Status
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    // Organisers can mark enrolments in their own events as completed.
    [HttpPut("{eventId:int}/enrolments/{enrolmentId:int}/complete")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CompleteEnrolment(
        int eventId,
        int enrolmentId)
    {
        var organiserId = GetOrganiserId();

        if (organiserId == null)
            return Unauthorized();

        var eventItem = await _context.Events
            .SingleOrDefaultAsync(e => e.EventId == eventId);

        if (eventItem == null)
            return NotFound(new { message = "Event not found." });

        if (eventItem.OrganiserId != organiserId.Value)
            return Forbid();

        var enrolment = await _context.Enrolments
            .SingleOrDefaultAsync(e =>
                e.EnrolmentId == enrolmentId &&
                e.EventId == eventId);

        if (enrolment == null)
        {
            return NotFound(new
            {
                message = "Enrolment not found for this event."
            });
        }

        if (string.Equals(
                enrolment.Status,
                "Completed",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "This enrolment is already completed."
            });
        }

        enrolment.Status = "Completed";

        await _context.SaveChangesAsync();

        return Ok(new
        {
            enrolment.EnrolmentId,
            enrolment.EventId,
            enrolment.Status
        });
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
            EventType = eventType,
            BannerImageUrl = request.BannerImageUrl
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
                eventItem.BannerImageUrl,
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
        eventItem.BannerImageUrl = request.BannerImageUrl;

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
            eventItem.BannerImageUrl,
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
    public string? BannerImageUrl { get; set; }
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