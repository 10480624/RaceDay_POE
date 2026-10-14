
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/enrolments")]
[Authorize(Roles = "Participant")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public EnrolmentsController(RaceDayContext context)
    {
        _context = context;
    }

    // A participant can view only their own enrolments.
    [HttpGet]
    public async Task<IActionResult> GetMyEnrolments()
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var enrolments = await _context.Enrolments
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.EnrolmentDate)
            .Select(e => new
            {
                e.EnrolmentId,
                e.UserId,
                e.EventId,
                e.CategoryId,
                e.EnrolmentDate,
                e.Status
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    // A participant can enrol themselves in an event category.
    [HttpPost]
    public async Task<IActionResult> CreateEnrolment(
        [FromBody] CreateEnrolmentRequest request)
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var eventExists = await _context.Events
            .AnyAsync(e => e.EventId == request.EventId);

        if (!eventExists)
        {
            return NotFound(new
            {
                message = "Event not found."
            });
        }

        var category = await _context.Categories
            .AsNoTracking()
            .SingleOrDefaultAsync(
                c => c.CategoryId == request.CategoryId);

        if (category == null)
        {
            return NotFound(new
            {
                message = "Category not found."
            });
        }

        if (category.EventId != request.EventId)
        {
            return BadRequest(new
            {
                message = "The category does not belong to this event."
            });
        }

        var alreadyEnrolled = await _context.Enrolments
            .AnyAsync(e =>
                e.UserId == userId &&
                e.EventId == request.EventId);

        if (alreadyEnrolled)
        {
            return Conflict(new
            {
                message = "You are already enrolled in this event."
            });
        }

        var enrolment = new Enrolment
        {
            UserId = userId,
            EventId = request.EventId,
            CategoryId = request.CategoryId,
            EnrolmentDate = DateTime.UtcNow,
            Status = "Pending"
        };

        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetMyEnrolments),
            new
            {
                enrolmentId = enrolment.EnrolmentId
            },
            new
            {
                enrolment.EnrolmentId,
                enrolment.UserId,
                enrolment.EventId,
                enrolment.CategoryId,
                enrolment.EnrolmentDate,
                enrolment.Status
            });
    }
}

public class CreateEnrolmentRequest
{
    public int EventId { get; set; }
    public int CategoryId { get; set; }
}
