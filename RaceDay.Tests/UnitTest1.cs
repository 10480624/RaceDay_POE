
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Tests;

public class EnrolmentsControllerTests
{
    [Fact]
    public async Task CreateEnrolment_ReturnsConflict_WhenParticipantAlreadyEnrolled()
    {
        // Arrange: create an isolated in-memory database.
        var options = new DbContextOptionsBuilder<RaceDayContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new RaceDayContext(options);

        var participant = new User
        {
            UserId = 3,
            Email = "participant.test@example.com",
            PasswordHash = "test-hash",
            FirstName = "Test",
            LastName = "Participant",
            Role = "Participant"
        };

        var raceEvent = new Event
        {
            EventId = 1,
            Name = "Test Run",
            EventDate = new DateTime(2026, 11, 15),
            Location = "Johannesburg",
            Distance = 5,
            EventType = "run"
        };

        var category = new Category
        {
            CategoryId = 1,
            EventId = 1,
            CategoryName = "5 km"
        };

        var existingEnrolment = new Enrolment
        {
            EnrolmentId = 1,
            UserId = 3,
            EventId = 1,
            CategoryId = 1,
            Status = "Pending"
        };

        context.Users.Add(participant);
        context.Events.Add(raceEvent);
        context.Categories.Add(category);
        context.Enrolments.Add(existingEnrolment);

        await context.SaveChangesAsync();

        var controller = new EnrolmentsController(context);

        // Simulate an authenticated participant.
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, "3"),
                            new Claim(ClaimTypes.Role, "Participant")
                        },
                        "TestAuthentication"))
            }
        };

        // Act: attempt to enrol in the same event again.
        var result = await controller.CreateEnrolment(
            new CreateEnrolmentRequest
            {
                EventId = 1,
                CategoryId = 1
            });

        // Assert: the controller must reject the duplicate.
        Assert.IsType<ConflictObjectResult>(result);

        // Confirm that no second enrolment was saved.
        Assert.Equal(1, await context.Enrolments.CountAsync());
    }
}
