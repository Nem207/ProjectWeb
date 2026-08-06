using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Features.Auth.Services;
using SpotifyClone.Features.MusicPlayer.Services;
namespace SpotifyClone.Features.MusicPlayer.Controllers
{
    [Authorize]
    public class HistoryController : Controller
    {
        private readonly IMusicPlayerService _musicPlayerService;
        private readonly ICurrentUserService _currentUser;
        public HistoryController(IMusicPlayerService musicPlayerService, ICurrentUserService currentUser)
        {
            _musicPlayerService = musicPlayerService;
            _currentUser = currentUser;
        }
        public async Task<IActionResult> Index()
        {
            if (_currentUser.UserId is not int userId)
            {
                return RedirectToAction("Login", "Auth");
            }
            var history = await _musicPlayerService.GetHistoryAsync(userId);
            return View(history);
        }
    }
}
