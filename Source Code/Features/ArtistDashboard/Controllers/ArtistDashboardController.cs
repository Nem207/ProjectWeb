using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Features.ArtistDashboard.Services;
using SpotifyClone.Features.Auth.Services;
namespace SpotifyClone.Features.ArtistDashboard.Controllers;
[Authorize(Roles = "Artist")]
public class ArtistDashboardController : Controller
{
    private readonly IArtistDashboardService _dashboardService;
    private readonly ICurrentUserService _currentUser;
    public ArtistDashboardController(
        IArtistDashboardService dashboardService,
        ICurrentUserService currentUser)
    {
        _dashboardService = dashboardService;
        _currentUser = currentUser;
    }
    public async Task<IActionResult> Index()
    {
        var userId = _currentUser.UserId;
        if (userId == null)
        {
            return Forbid();
        }
        var model = await _dashboardService.GetDashboardForUserAsync(userId.Value);
        if (model == null)
        {
            return View("NotLinked");
        }
        return View(model);
    }
    public async Task<IActionResult> UploadSong()
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Forbid();
        var isLinked = await _dashboardService.GetDashboardForUserAsync(userId.Value) != null;
        if (!isLinked)
        {
            return View("NotLinked");
        }
        return View(new SpotifyClone.Features.ArtistDashboard.ViewModels.UploadSongViewModel());
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadSong(SpotifyClone.Features.ArtistDashboard.ViewModels.UploadSongViewModel input)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Forbid();
        if (!ModelState.IsValid)
        {
            return View(input);
        }
        var success = await _dashboardService.CreateSongAsync(userId.Value, input);
        if (!success)
        {
            return View("NotLinked");
        }
        TempData["SuccessMessage"] = "Da gui bai hat, cho Admin duyet.";
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Notifications()
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Forbid();
        var model = await _dashboardService.GetNotificationsForUserAsync(userId.Value);
        if (model == null)
        {
            return View("NotLinked");
        }
        return View(model);
    }
}