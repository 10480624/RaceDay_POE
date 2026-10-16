using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Tests;

public class EnrolmentsControllerTests
{
    [Fact]
    public async Task CreateEnrolment_ReturnsConflict_WhenParticipantAlreadyEnrolled()
    {
        using var context = CreateContext();

        context.Users.Add(CreateUser(3, "participant.test@example.com", "Participant"));
        context.Events.Add(CreateEvent(1, 2));
        context.Categories.Add(new Category
        {
            CategoryId = 1,
            EventId = 1,
            CategoryName = "5 km"
        });
        context.Enrolments.Add(new Enrolment
        {
            EnrolmentId = 1,
            UserId = 3,
            EventId = 1,
            CategoryId = 1,
            Status = "Pending"
        });
        await context.SaveChangesAsync();

        var controller = new EnrolmentsController(context);
        SetUser(controller, 3, "Participant");

        var result = await controller.CreateEnrolment(
            new CreateEnrolmentRequest { EventId = 1, CategoryId = 1 });

        Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(1, await context.Enrolments.CountAsync());
    }

    private static RaceDayContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RaceDayContext(options);
    }

    private static User CreateUser(int id, string email, string role)
    {
        return new User
        {
            UserId = id,
            Email = email,
            PasswordHash = "placeholder",
            FirstName = "Test",
            LastName = role,
            Role = role
        };
    }

    private static Event CreateEvent(int id, int organiserId)
    {
        return new Event
        {
            EventId = id,
            OrganiserId = organiserId,
            Name = "Test Run",
            EventDate = DateTime.UtcNow.Date.AddDays(30),
            Location = "Johannesburg",
            Distance = 5,
            EventType = "run"
        };
    }

    private static void SetUser(ControllerBase controller, int userId, string role)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                            new Claim(ClaimTypes.Role, role)
                        },
                        "TestAuthentication"))
            }
        };
    }
}

public class AuthControllerTests
{
    [Fact]
    public async Task Register_ReturnsCreated_WhenParticipantDetailsAreValid()
    {
        using var context = CreateContext();
        var controller = new AuthController(context);

        var result = await controller.Register(new RegisterRequest
        {
            Email = " NEW.PARTICIPANT@example.com ",
            Password = "SecureTest123!",
            FirstName = "New",
            LastName = "Participant",
            Role = "Participant"
        });

        var created = Assert.IsType<CreatedResult>(result);
        Assert.Equal(1, await context.Users.CountAsync());

        var savedUser = await context.Users.SingleAsync();
        Assert.Equal("new.participant@example.com", savedUser.Email);
        Assert.Equal("Participant", savedUser.Role);
        Assert.NotEqual("SecureTest123!", savedUser.PasswordHash);
    }

    [Fact]
    public async Task Register_ReturnsConflict_WhenEmailAlreadyExists()
    {
        using var context = CreateContext();
        context.Users.Add(CreateUser(1, "existing@example.com", "Participant"));
        await context.SaveChangesAsync();

        var controller = new AuthController(context);

        var result = await controller.Register(new RegisterRequest
        {
            Email = "EXISTING@example.com",
            Password = "SecureTest123!",
            FirstName = "Existing",
            LastName = "Participant",
            Role = "Participant"
        });

        Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(1, await context.Users.CountAsync());
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenPublicRegistrationRequestsOrganiser()
    {
        using var context = CreateContext();
        var controller = new AuthController(context);

        var result = await controller.Register(new RegisterRequest
        {
            Email = "new.organiser@example.com",
            Password = "SecureTest123!",
            FirstName = "New",
            LastName = "Organiser",
            Role = "Organiser"
        });

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(await context.Users.ToListAsync());
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenEmailDoesNotExist()
    {
        using var context = CreateContext();
        var controller = CreateAuthController(context);

        var result = await controller.Login(new LoginRequest
        {
            Email = "missing@example.com",
            Password = "SecureTest123!"
        });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsIncorrect()
    {
        using var context = CreateContext();
        var user = CreateUser(1, "participant@example.com", "Participant");
        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, "CorrectPassword123!");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = CreateAuthController(context);

        var result = await controller.Login(new LoginRequest
        {
            Email = user.Email,
            Password = "WrongPassword123!"
        });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsToken_WhenCredentialsAreValid()
    {
        using var context = CreateContext();
        var user = CreateUser(1, "participant@example.com", "Participant");
        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, "CorrectPassword123!");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = CreateAuthController(context);

        var result = await controller.Login(new LoginRequest
        {
            Email = user.Email,
            Password = "CorrectPassword123!"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);

        var tokenProperty = ok.Value.GetType().GetProperty("token");
        Assert.NotNull(tokenProperty);
        Assert.False(string.IsNullOrWhiteSpace(
            tokenProperty!.GetValue(ok.Value)?.ToString()));
    }

    private static AuthController CreateAuthController(RaceDayContext context)
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "RaceDay-Test-Signing-Key-12345678901234567890",
            ["Jwt:Issuer"] = "RaceDay.Api",
            ["Jwt:Audience"] = "RaceDay.Client",
            ["Jwt:ExpiryMinutes"] = "60"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = new Microsoft.Extensions.DependencyInjection.ServiceCollection()
            .AddSingleton(configuration)
            .BuildServiceProvider();

        var controller = new AuthController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };

        return controller;
    }

    private static RaceDayContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RaceDayContext(options);
    }

    private static User CreateUser(int id, string email, string role)
    {
        return new User
        {
            UserId = id,
            Email = email,
            PasswordHash = "placeholder",
            FirstName = "Test",
            LastName = role,
            Role = role
        };
    }
}

public class EventsControllerTests
{
    [Fact]
    public async Task GetEvents_ReturnsEvents()
    {
        using var context = CreateContext();
        context.Events.Add(CreateEvent(1, 2));
        await context.SaveChangesAsync();

        var controller = new EventsController(context);

        var result = await controller.GetEvents();

        var ok = Assert.IsType<OkObjectResult>(result);
        var events = Assert.IsAssignableFrom<System.Collections.IEnumerable>(ok.Value);
        Assert.NotNull(events);
    }

    [Fact]
    public async Task GetEvent_ReturnsNotFound_WhenEventDoesNotExist()
    {
        using var context = CreateContext();
        var controller = new EventsController(context);

        var result = await controller.GetEvent(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateEvent_ReturnsUnauthorized_WhenOrganiserClaimIsMissing()
    {
        using var context = CreateContext();
        var controller = new EventsController(context);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var result = await controller.CreateEvent(new CreateEventRequest
        {
            Name = "Test Run",
            EventDate = DateTime.UtcNow.Date.AddDays(30),
            Location = "Johannesburg",
            Distance = 5,
            EventType = "run"
        });

        Assert.IsType<UnauthorizedResult>(result);
        Assert.Empty(await context.Events.ToListAsync());
    }

    private static RaceDayContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RaceDayContext(options);
    }

    private static Event CreateEvent(int id, int organiserId)
    {
        return new Event
        {
            EventId = id,
            OrganiserId = organiserId,
            Name = "Test Run",
            EventDate = DateTime.UtcNow.Date.AddDays(30),
            Location = "Johannesburg",
            Distance = 5,
            EventType = "run"
        };
    }
}
