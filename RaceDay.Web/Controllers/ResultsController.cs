using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class ResultsController : Controller
{
    private readonly ApiClient _api;

    public ResultsController(ApiClient api)
    {
        _api = api;
    }

    private bool IsOrganiser() =>
        HttpContext.Session.GetString("Role") == "Organiser";

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var role = HttpContext.Session.GetString("Role");

        if (role != "Participant" && role != "Organiser")
            return RedirectToAction("Login", "Account");

        var results = await _api.GetAsync<List<ResultViewModel>>("/api/results");

        return View(results ?? new List<ResultViewModel>());
    }

    [HttpGet]
    public async Task<IActionResult> Create(int eventId, int enrolmentId)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        var eventItem = await _api.GetAsync<EventViewModel>(
            $"/api/events/{eventId}");

        if (eventItem == null)
            return NotFound();

        var enrolments = await _api.GetAsync<List<OrganiserEnrolmentViewModel>>(
            $"/api/events/{eventId}/enrolments");

        var enrolment = enrolments?.FirstOrDefault(
            e => e.EnrolmentId == enrolmentId);

        if (enrolment == null)
            return NotFound();

        if (!string.Equals(
            enrolment.Status,
            "Completed",
            StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Only completed enrolments can receive results.";
            return RedirectToAction("Index", "ManageEnrolments", new { eventId });
        }

        var existingResults = await _api.GetAsync<List<ResultViewModel>>(
            "/api/results");

        if (existingResults?.Any(r => r.EnrolmentId == enrolmentId) == true)
        {
            TempData["Error"] = "A result already exists for this enrolment.";
            return RedirectToAction("Index", "ManageEnrolments", new { eventId });
        }

        var model = new CreateResultViewModel
        {
            EventId = eventId,
            EventName = eventItem.Name,
            EnrolmentId = enrolmentId
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateResultViewModel model)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
            return View(model);

        var result = await _api.PostAsync<ResultViewModel>(
            "/api/results",
            new
            {
                model.EnrolmentId,
                model.FinishTime,
                model.FinishingPosition
            });

        if (result == null)
        {
            ModelState.AddModelError(
                "",
                "The result could not be saved. Check that the enrolment is completed and that no result already exists.");

            return View(model);
        }

        TempData["Message"] = "Result saved successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        var result = await _api.GetAsync<ResultViewModel>(
            $"/api/results/{id}");

        if (result == null)
            return NotFound();

        var model = new EditResultViewModel
        {
            ResultId = result.ResultId,
            FinishTime = result.FinishTime,
            FinishingPosition = result.FinishingPosition
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditResultViewModel model)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
            return View(model);

        var result = await _api.PutAsync<ResultViewModel>(
            $"/api/results/{model.ResultId}",
            new
            {
                model.FinishTime,
                model.FinishingPosition
            });

        if (result == null)
        {
            ModelState.AddModelError(
                "",
                "The result could not be updated. Please check the values and try again.");

            return View(model);
        }

        TempData["Message"] = "Result updated successfully.";

        return RedirectToAction(nameof(Index));
    }
}