using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class EventsController : Controller
{
    private readonly ApiClient _api;

    public EventsController(ApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var events = await _api.GetAsync<List<EventViewModel>>("/api/events");

        return View(events ?? new List<EventViewModel>());
    }

    public async Task<IActionResult> Details(int id)
    {
        var eventItem = await _api.GetAsync<EventViewModel>($"/api/events/{id}");

        if (eventItem == null)
            return NotFound();

        var categories = await _api.GetAsync<List<CategoryViewModel>>(
            $"/api/events/{id}/categories");

        eventItem.Categories = categories ?? new List<CategoryViewModel>();

        return View(eventItem);
    }
}