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
        if (request.EventDate.Date < DateTime.UtcNow.Date)
            return BadRequest(new { message = "Event date cannot be in the past." });

        var eventType = request.EventType.Trim().ToLowerInvariant();

        if (eventType is not ("run" or "walk" or "cycle"))
            return BadRequest(new { message = "Event type must be run, walk, or cycle." });

        if (request.Distance <= 0)
            return BadRequest(new { message = "Distance must be greater than zero." });

        var organiserIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(organiserIdValue, out var organiserId))
            return Unauthorized();

        var eventItem = new Event
        {
            OrganiserId = organiserId,
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
