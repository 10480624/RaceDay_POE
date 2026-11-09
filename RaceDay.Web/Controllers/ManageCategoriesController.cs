using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class ManageCategoriesController : Controller
{
    private readonly ApiClient _api;

    public ManageCategoriesController(ApiClient api)
    {
        _api = api;
    }

    private bool IsOrganiser()
    {
        return HttpContext.Session.GetString("Role") == "Organiser";
    }

    [HttpGet]
    public async Task<IActionResult> Index(int eventId)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        var eventItem = await _api.GetAsync<EventViewModel>(
            $"/api/events/{eventId}");

        if (eventItem == null)
            return NotFound();

        var categories = await _api.GetAsync<List<CategoryViewModel>>(
            $"/api/events/{eventId}/categories");

        var model = new CategoryManagementViewModel
        {
            EventId = eventId,
            EventName = eventItem.Name,
            Categories = categories ?? new List<CategoryViewModel>(),
            NewCategory = new CreateCategoryViewModel
            {
                EventId = eventId
            }
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryManagementViewModel model)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        model.NewCategory.EventId = model.EventId;

        if (!ModelState.IsValid)
        {
            var eventItem = await _api.GetAsync<EventViewModel>(
                $"/api/events/{model.EventId}");

            model.EventName = eventItem?.Name ?? "Event";

            model.Categories = await _api.GetAsync<List<CategoryViewModel>>(
                $"/api/events/{model.EventId}/categories")
                ?? new List<CategoryViewModel>();

            return View("Index", model);
        }

        var result = await _api.PostAsync<CategoryViewModel>(
            $"/api/events/{model.EventId}/categories",
            new
            {
                model.NewCategory.CategoryName,
                model.NewCategory.AgeRange,
                model.NewCategory.Distance
            });

        if (result == null)
        {
            TempData["Error"] = "Could not add the category. Check the details and try again.";
        }
        else
        {
            TempData["Message"] = "Category added successfully.";
        }

        return RedirectToAction(nameof(Index), new { eventId = model.EventId });
    }
}