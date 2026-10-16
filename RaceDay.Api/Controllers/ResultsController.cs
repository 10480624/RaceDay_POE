using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/results")]
[Authorize]
public class ResultsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public ResultsController(RaceDayContext context)
    {
        _context = context;
    }

    // GET: api/results
    // Participants see their own results.
    // Organisers see results for their own events.
    [HttpGet]
    [Authorize(Roles = "Participant,Organiser")]
    public async Task<IActionResult> GetResults()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var query = _context.Results
            .AsNoTracking()
            .Include(r => r.Enrolment)
            .ThenInclude(e => e.Event)
            .Include(r => r.Enrolment)
            .ThenInclude(e => e.User)
            .AsQueryable();

        if (User.IsInRole("Participant"))
        {
            query = query.Where(r => r.Enrolment.UserId == userId);
        }
        else if (User.IsInRole("Organiser"))
        {
            query = query.Where(
                r => r.Enrolment.Event.OrganiserId == userId);
        }
        else
        {
            return Forbid();
        }

        var results = await query
            .OrderBy(r => r.FinishingPosition)
            .Select(r => new
            {
                r.ResultId,
                r.EnrolmentId,
                r.FinishTime,
                r.FinishingPosition,
                r.Enrolment.EventId,
                EventName = r.Enrolment.Event.Name,
                r.Enrolment.CategoryId,
                ParticipantName =
                    r.Enrolment.User.FirstName + " " +
                    r.Enrolment.User.LastName
            })
            .ToListAsync();

        return Ok(results);
    }

    // GET: api/results/{id}
    // A participant can access their own result.
    // An organiser can access results for their own events.
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Participant,Organiser")]
    public async Task<IActionResult> GetResult(int id)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var result = await _context.Results
            .AsNoTracking()
            .Include(r => r.Enrolment)
            .ThenInclude(e => e.Event)
            .Include(r => r.Enrolment)
            .ThenInclude(e => e.User)
            .SingleOrDefaultAsync(r => r.ResultId == id);

        if (result == null)
            return NotFound(new { message = "Result not found." });

        var isParticipantOwner =
            User.IsInRole("Participant") &&
            result.Enrolment.UserId == userId;

        var isEventOrganiser =
            User.IsInRole("Organiser") &&
            result.Enrolment.Event.OrganiserId == userId;

        if (!isParticipantOwner && !isEventOrganiser)
            return Forbid();

        return Ok(new
        {
            result.ResultId,
            result.EnrolmentId,
            result.FinishTime,
            result.FinishingPosition,
            result.Enrolment.EventId,
            EventName = result.Enrolment.Event.Name,
            result.Enrolment.CategoryId,
            ParticipantName =
                result.Enrolment.User.FirstName + " " +
                result.Enrolment.User.LastName
        });
    }

    // POST: api/results
    // Only the organiser of the event can record a result.
    // The enrolment must have completed status.
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateResult(
        [FromBody] CreateResultRequest request)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        if (request.EnrolmentId <= 0)
        {
            return BadRequest(new
            {
                message = "A valid enrolment ID is required."
            });
        }

        if (request.FinishTime == null ||
            request.FinishTime <= TimeSpan.Zero ||
            request.FinishTime >= TimeSpan.FromDays(1))
        {
            return BadRequest(new
            {
                message =
                    "Finish time must be greater than zero and less than 24 hours."
            });
        }

        if (request.FinishingPosition == null ||
            request.FinishingPosition <= 0)
        {
            return BadRequest(new
            {
                message = "Finishing position must be greater than zero."
            });
        }

        var enrolment = await _context.Enrolments
            .Include(e => e.Event)
            .SingleOrDefaultAsync(
                e => e.EnrolmentId == request.EnrolmentId);

        if (enrolment == null)
        {
            return NotFound(new
            {
                message = "Enrolment not found."
            });
        }

        if (enrolment.Event.OrganiserId != userId)
            return Forbid();

        if (!string.Equals(
                enrolment.Status,
                "Completed",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message =
                    "A result can only be recorded for a completed enrolment."
            });
        }

        var resultAlreadyExists = await _context.Results
            .AnyAsync(r => r.EnrolmentId == request.EnrolmentId);

        if (resultAlreadyExists)
        {
            return Conflict(new
            {
                message =
                    "A result already exists for this enrolment."
            });
        }

        var result = new Result
        {
            EnrolmentId = request.EnrolmentId,
            FinishTime = request.FinishTime,
            FinishingPosition = request.FinishingPosition
        };

        _context.Results.Add(result);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetResult),
            new { id = result.ResultId },
            new
            {
                result.ResultId,
                result.EnrolmentId,
                result.FinishTime,
                result.FinishingPosition
            });
    }

    // PUT: api/results/{id}
    // Only the organiser of the event can update its result.
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateResult(
        int id,
        [FromBody] UpdateResultRequest request)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        if (request.FinishTime == null ||
            request.FinishTime <= TimeSpan.Zero ||
            request.FinishTime >= TimeSpan.FromDays(1))
        {
            return BadRequest(new
            {
                message =
                    "Finish time must be greater than zero and less than 24 hours."
            });
        }

        if (request.FinishingPosition == null ||
            request.FinishingPosition <= 0)
        {
            return BadRequest(new
            {
                message = "Finishing position must be greater than zero."
            });
        }

        var result = await _context.Results
            .Include(r => r.Enrolment)
            .ThenInclude(e => e.Event)
            .SingleOrDefaultAsync(r => r.ResultId == id);

        if (result == null)
        {
            return NotFound(new { message = "Result not found." });
        }

        if (result.Enrolment.Event.OrganiserId != userId)
            return Forbid();

        result.FinishTime = request.FinishTime;
        result.FinishingPosition = request.FinishingPosition;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            result.ResultId,
            result.EnrolmentId,
            result.FinishTime,
            result.FinishingPosition
        });
    }

    private bool TryGetUserId(out int userId)
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userIdValue, out userId);
    }
}

public class CreateResultRequest
{
    public int EnrolmentId { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? FinishingPosition { get; set; }
}

public class UpdateResultRequest
{
    public TimeSpan? FinishTime { get; set; }
    public int? FinishingPosition { get; set; }
}
