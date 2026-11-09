using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class ProfileController : Controller
{
    private readonly ApiClient _api;

    public ProfileController(ApiClient api)
    {
        _api = api;
    }

    private bool IsAuthenticated() =>
        !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("Token"));

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!IsAuthenticated())
            return RedirectToAction("Login", "Account");

        var profile = await _api.GetAsync<ProfileViewModel>("/api/profile");

        if (profile == null)
        {
            TempData["Error"] = "Unable to retrieve your profile. Please try again.";
            return RedirectToAction("Index", "Events");
        }

        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel model)
    {
        if (!IsAuthenticated())
            return RedirectToAction("Login", "Account");

        if (model.DateOfBirth.HasValue &&
            model.DateOfBirth.Value.Date > DateTime.UtcNow.Date)
        {
            ModelState.AddModelError(
                nameof(model.DateOfBirth),
                "Date of birth cannot be in the future.");
        }

        if (!ModelState.IsValid)
            return View(model);

        var updatedProfile = await _api.PutAsync<ProfileViewModel>(
            "/api/profile",
            new
            {
                model.FirstName,
                model.LastName,
                model.DateOfBirth,
                model.EmergencyContactName,
                model.EmergencyContactPhone,
                model.MedicalConditions,
                model.ProfilePictureUrl
            });

        if (updatedProfile == null)
        {
            ModelState.AddModelError(
                "",
                "Your profile could not be updated. Check your details and try again.");
            return View(model);
        }

        HttpContext.Session.SetString("Name", updatedProfile.FirstName);
        TempData["Message"] = "Your profile has been updated successfully.";
        return RedirectToAction(nameof(Index));
    }
}
