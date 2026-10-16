
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly RaceDayContext _context;

    public ProfileController(RaceDayContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();

        if (userId == null)
            return Unauthorized();

        var user = await _context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.UserId == userId.Value);

        if (user == null)
            return NotFound(new { message = "User account not found." });

        var profile = await _context.Profiles
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.UserId == userId.Value);

        return Ok(new
        {
            user.UserId,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role,
            DateOfBirth = profile?.DateOfBirth,
            EmergencyContactName = profile?.EmergencyContactName,
            EmergencyContactPhone = profile?.EmergencyContactPhone,
            MedicalConditions = profile?.MedicalConditions,
            ProfilePictureUrl = profile?.ProfilePictureUrl
        });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserId();

        if (userId == null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.FirstName))
            return BadRequest(new { message = "First name is required." });

        if (string.IsNullOrWhiteSpace(request.LastName))
            return BadRequest(new { message = "Last name is required." });

        if (request.FirstName.Trim().Length > 100 ||
            request.LastName.Trim().Length > 100)
            return BadRequest(new { message = "Names cannot exceed 100 characters." });

        if (request.EmergencyContactName?.Length > 100)
            return BadRequest(new { message = "Emergency contact name cannot exceed 100 characters." });

        if (request.EmergencyContactPhone?.Length > 20)
            return BadRequest(new { message = "Emergency contact phone cannot exceed 20 characters." });

        if (request.MedicalConditions?.Length > 500)
            return BadRequest(new { message = "Medical conditions cannot exceed 500 characters." });

        if (request.ProfilePictureUrl?.Length > 500)
            return BadRequest(new { message = "Profile picture URL cannot exceed 500 characters." });

        if (request.DateOfBirth.HasValue &&
            request.DateOfBirth.Value.Date > DateTime.UtcNow.Date)
            return BadRequest(new { message = "Date of birth cannot be in the future." });

        var user = await _context.Users
            .SingleOrDefaultAsync(u => u.UserId == userId.Value);

        if (user == null)
            return NotFound(new { message = "User account not found." });

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();

        var profile = await _context.Profiles
            .SingleOrDefaultAsync(p => p.UserId == userId.Value);

        if (profile == null)
        {
            profile = new Profile
            {
                UserId = userId.Value
            };

            _context.Profiles.Add(profile);
        }

        profile.DateOfBirth = request.DateOfBirth?.Date;
        profile.EmergencyContactName = request.EmergencyContactName?.Trim();
        profile.EmergencyContactPhone = request.EmergencyContactPhone?.Trim();
        profile.MedicalConditions = request.MedicalConditions?.Trim();
        profile.ProfilePictureUrl = request.ProfilePictureUrl?.Trim();

        await _context.SaveChangesAsync();

        return Ok(new
        {
            user.UserId,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role,
            profile.DateOfBirth,
            profile.EmergencyContactName,
            profile.EmergencyContactPhone,
            profile.MedicalConditions,
            profile.ProfilePictureUrl
        });
    }

    private int? GetUserId()
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.TryParse(userIdValue, out var userId)
            ? userId
            : null;
    }
}

public class UpdateProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? MedicalConditions { get; set; }
    public string? ProfilePictureUrl { get; set; }
}
