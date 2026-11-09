using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class EnrolmentsController : Controller
{
    private readonly ApiClient _api;

    public EnrolmentsController(ApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString("Token") == null)
            return RedirectToAction("Login", "Account");

        var enrolments = await _api.GetAsync<List<EnrolmentViewModel>>(
            "/api/enrolments");

        return View(enrolments ?? new List<EnrolmentViewModel>());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int eventId, int categoryId)
    {
        if (HttpContext.Session.GetString("Token") == null)
            return RedirectToAction("Login", "Account");

        var result = await _api.PostAsync<EnrolmentViewModel>(
            "/api/enrolments",
            new
            {
                EventId = eventId,
                CategoryId = categoryId
            });

        if (result == null)
            TempData["Error"] = "Enrolment failed. You may already be enrolled, or the selected category may be invalid.";
        else
            TempData["Message"] = "You have successfully enrolled in this event.";

        return RedirectToAction("Details", "Events", new { id = eventId });
    }
}