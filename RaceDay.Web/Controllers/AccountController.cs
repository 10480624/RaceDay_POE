using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class AccountController : Controller
{
    private readonly ApiClient _api;

    public AccountController(ApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _api.PostAsync<LoginResponse>(
            "/api/auth/login",
            new { model.Email, model.Password });

        if (result == null || string.IsNullOrWhiteSpace(result.Token))
        {
            ModelState.AddModelError("", "Login failed. Check your details and try again.");
            return View(model);
        }

        HttpContext.Session.SetString("Token", result.Token);
        HttpContext.Session.SetString("Role", result.User.Role);
        HttpContext.Session.SetString("UserId", result.User.UserId.ToString());
        HttpContext.Session.SetString("Name", result.User.FirstName);

        return RedirectToAction("Index", "Events");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _api.PostAsync<RegisterResponse>(
            "/api/auth/register",
            new
            {
                model.Email,
                model.Password,
                model.FirstName,
                model.LastName,
                Role = "Participant"
            });

        if (result == null)
        {
            ModelState.AddModelError(
                "",
                "Registration failed. The email may already be registered, or the API may be unavailable.");

            return View(model);
        }

        TempData["Message"] = "Registration successful. Please log in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}