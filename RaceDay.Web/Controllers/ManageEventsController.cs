using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class ManageEventsController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebHostEnvironment _environment;

    public ManageEventsController(ApiClient api, IWebHostEnvironment environment)
    {
        _api = api;
        _environment = environment;
    }

    private bool IsOrganiser() => HttpContext.Session.GetString("Role") == "Organiser";

    private async Task<string?> SaveBanner(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;
        if (file.Length > 5 * 1024 * 1024) throw new InvalidOperationException("Banner image must be 5 MB or smaller.");
        var extensions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        { [".jpg"]="image/jpeg", [".jpeg"]="image/jpeg", [".png"]="image/png", [".webp"]="image/webp" };
        var ext = Path.GetExtension(file.FileName);
        if (!extensions.TryGetValue(ext, out var mime) || !string.Equals(file.ContentType, mime, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Upload a JPG, PNG or WEBP image.");
        var root = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var folder = Path.Combine(root, "uploads", "event-banners");
        Directory.CreateDirectory(folder);
        var name = Guid.NewGuid().ToString("N") + ext.ToLowerInvariant();
        await using var stream = System.IO.File.Create(Path.Combine(folder, name));
        await file.CopyToAsync(stream);
        return "/uploads/event-banners/" + name;
    }

    private async Task<List<EventViewModel>> MyEvents()
    {
        var all = await _api.GetAsync<List<EventViewModel>>("/api/events") ?? new();
        var id = HttpContext.Session.GetString("UserId");
        return all.Where(e => e.OrganiserId.ToString() == id).OrderBy(e => e.EventDate).ToList();
    }

    public async Task<IActionResult> Index()
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Account");
        return View(await MyEvents());
    }

    public async Task<IActionResult> Dashboard()
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Account");
        var events = await MyEvents();
        var counts = new Dictionary<int, int>();
        foreach (var e in events)
        {
            var enrolments = await _api.GetAsync<List<OrganiserEnrolmentViewModel>>($"/api/events/{e.EventId}/enrolments");
            counts[e.EventId] = enrolments?.Count ?? 0;
        }
        ViewBag.EnrolmentCounts = counts;
        ViewBag.TotalEnrolments = counts.Values.Sum();
        ViewBag.UpcomingEvents = events.Count(e => e.EventDate.Date >= DateTime.Today);
        return View(events);
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Account");
        return View(new OrganiserEventViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrganiserEventViewModel model, IFormFile? bannerImage)
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(model);
        try { model.BannerImageUrl = await SaveBanner(bannerImage); }
        catch (InvalidOperationException ex) { ModelState.AddModelError("bannerImage", ex.Message); return View(model); }
        var created = await _api.PostAsync<EventViewModel>("/api/events", model);
        if (created == null) { ModelState.AddModelError("", "Could not create the event."); return View(model); }
        TempData["Message"] = "Event created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Account");
        var e = await _api.GetAsync<EventViewModel>($"/api/events/{id}");
        if (e == null) return NotFound();
        var model = new OrganiserEventViewModel { EventId=e.EventId, Name=e.Name, Description=e.Description, EventDate=e.EventDate, Location=e.Location, Distance=e.Distance, EventType=e.EventType, BannerImageUrl=e.BannerImageUrl };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(OrganiserEventViewModel model, IFormFile? bannerImage)
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(model);
        try { var uploaded = await SaveBanner(bannerImage); if (uploaded != null) model.BannerImageUrl = uploaded; }
        catch (InvalidOperationException ex) { ModelState.AddModelError("bannerImage", ex.Message); return View(model); }
        var updated = await _api.PutAsync<EventViewModel>($"/api/events/{model.EventId}", model);
        if (updated == null) { ModelState.AddModelError("", "Could not update the event."); return View(model); }
        TempData["Message"] = "Event updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Account");
        var ok = await _api.DeleteAsync($"/api/events/{id}");
        TempData[ok ? "Message" : "Error"] = ok ? "Event deleted successfully." : "Could not delete the event. It may have existing enrolments.";
        return RedirectToAction(nameof(Index));
    }
}