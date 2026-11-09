using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class ManageEventsController : Controller
{
    private readonly ApiClient _api;

    public ManageEventsController(ApiClient api)
    {
        _api = api;
    }

    private bool IsOrganiser()
    {
        return HttpContext.Session.GetString("Role") == "Organiser";
    }

    public async Task<IActionResult> Index()
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        var events = await _api.GetAsync<List<EventViewModel>>("/api/events");

        var userId = HttpContext.Session.GetString("UserId");

        var myEvents = events?
            .Where(e => e.OrganiserId.ToString() == userId)
            .ToList() ?? new List<EventViewModel>();

        return View(myEvents);
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        return View(new OrganiserEventViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrganiserEventViewModel model)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
            return View(model);

        var result = await _api.PostAsync<EventViewModel>(
            "/api/events",
            model);

        if (result == null)
        {
            ModelState.AddModelError("", "Could not create the event. Check the details and try again.");
            return View(model);
        }

        TempData["Message"] = "Event created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        var eventItem = await _api.GetAsync<EventViewModel>($"/api/events/{id}");

        if (eventItem == null)
            return NotFound();

        var model = new OrganiserEventViewModel
        {
            EventId = eventItem.EventId,
            Name = eventItem.Name,
            Description = eventItem.Description,
            EventDate = eventItem.EventDate,
            Location = eventItem.Location,
            Distance = eventItem.Distance,
            EventType = eventItem.EventType
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(OrganiserEventViewModel model)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
            return View(model);

        var result = await _api.PutAsync<EventViewModel>(
            $"/api/events/{model.EventId}",
            model);

        if (result == null)
        {
            ModelState.AddModelError("", "Could not update the event.");
            return View(model);
        }

        TempData["Message"] = "Event updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        var success = await _api.DeleteAsync($"/api/events/{id}");

        TempData[success ? "Message" : "Error"] = success
            ? "Event deleted successfully."
            : "Could not delete the event. It may have existing enrolments.";

        return RedirectToAction(nameof(Index));
    }
}