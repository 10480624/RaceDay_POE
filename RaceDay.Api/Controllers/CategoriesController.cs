
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/categories")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayContext _context;

    public CategoriesController(RaceDayContext context)
    {
        _context = context;
    }

    // Anyone can view categories for an event.
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories(int eventId)
    {
        var eventExists = await _context.Events
            .AnyAsync(e => e.EventId == eventId);

        if (!eventExists)
            return NotFound(new { message = "Event not found." });

        var categories = await _context.Categories
            .AsNoTracking()
            .Where(c => c.EventId == eventId)
            .OrderBy(c => c.CategoryName)
            .Select(c => new
            {
                c.CategoryId,
                c.EventId,
                c.CategoryName,
                c.AgeRange,
                c.Distance
            })
            .ToListAsync();

        return Ok(categories);
    }

    // Only the organiser who owns the event can add its categories.
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateCategory(
        int eventId,
        [FromBody] CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CategoryName))
        {
            return BadRequest(new
            {
                message = "Category name is required."
            });
        }

        if (request.Distance.HasValue && request.Distance.Value <= 0)
        {
            return BadRequest(new
            {
                message = "Category distance must be greater than zero."
            });
        }

        var eventItem = await _context.Events
            .SingleOrDefaultAsync(e => e.EventId == eventId);

        if (eventItem == null)
            return NotFound(new { message = "Event not found." });

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var organiserId))
            return Unauthorized();

        if (eventItem.OrganiserId != organiserId)
            return Forbid();

        var category = new Category
        {
            EventId = eventId,
            CategoryName = request.CategoryName.Trim(),
            AgeRange = string.IsNullOrWhiteSpace(request.AgeRange)
                ? null
                : request.AgeRange.Trim(),
            Distance = request.Distance
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCategories),
            new { eventId },
            new
            {
                category.CategoryId,
                category.EventId,
                category.CategoryName,
                category.AgeRange,
                category.Distance
            });
    }
}

public class CreateCategoryRequest
{
    public string CategoryName { get; set; } = string.Empty;
    public string? AgeRange { get; set; }
    public decimal? Distance { get; set; }
}
