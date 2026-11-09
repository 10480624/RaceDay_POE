using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class ManageEnrolmentsController : Controller
{
    private readonly ApiClient _api;

    public ManageEnrolmentsController(ApiClient api)
    {
        _api = api;
    }

    private bool IsOrganiser() =>
        HttpContext.Session.GetString("Role") == "Organiser";

    [HttpGet]
    public async Task<IActionResult> Index(int eventId)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        var eventItem = await _api.GetAsync<EventViewModel>(
            $"/api/events/{eventId}");

        if (eventItem == null)
            return NotFound();

        var enrolments = await _api.GetAsync<List<OrganiserEnrolmentViewModel>>(
            $"/api/events/{eventId}/enrolments");

        var results = await _api.GetAsync<List<ResultViewModel>>(
            "/api/results");

        var enrolmentList = enrolments ?? new List<OrganiserEnrolmentViewModel>();

        foreach (var enrolment in enrolmentList)
        {
            enrolment.ResultId = results?
                .FirstOrDefault(r => r.EnrolmentId == enrolment.EnrolmentId)?
                .ResultId;
        }

        var model = new ManageEnrolmentsViewModel
        {
            EventId = eventId,
            EventName = eventItem.Name,
            Enrolments = enrolmentList
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int eventId, int enrolmentId)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        var success = await _api.PutAsync<object>(
            $"/api/events/{eventId}/enrolments/{enrolmentId}/complete",
            new { });

        if (success == null)
            TempData["Error"] = "Could not mark the enrolment as completed.";
        else
            TempData["Message"] = "Enrolment marked as completed.";

        return RedirectToAction(nameof(Index), new { eventId });
    }
}