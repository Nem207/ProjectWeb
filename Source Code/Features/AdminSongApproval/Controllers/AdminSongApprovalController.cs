using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Features.AdminSongApproval.Services;
namespace SpotifyClone.Features.AdminSongApproval.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminSongApprovalController : Controller
    {
        private readonly SongApprovalService _songApprovalService;
        public AdminSongApprovalController(SongApprovalService songApprovalService)
        {
            _songApprovalService = songApprovalService;
        }
        public async Task<IActionResult> Index()
        {
            var pendingSongs = await _songApprovalService.GetPendingSongsAsync();
            return View(pendingSongs);
        }
        [HttpPost]
        [Route("AdminSongApproval/Approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            var (success, message) = await _songApprovalService.ApproveSongAsync(id);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }
        [HttpPost]
        [Route("AdminSongApproval/Reject/{id}")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectRequest request)
        {
            var (success, message) = await _songApprovalService.RejectSongAsync(id, request.Reason);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }
    }
    public class RejectRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}