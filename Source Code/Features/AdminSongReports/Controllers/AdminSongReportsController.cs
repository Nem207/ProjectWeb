using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Features.Auth.Services;
using SpotifyClone.Features.SongReports.Services;
namespace SpotifyClone.Features.AdminSongReports.Controllers;

[Authorize(Roles = "Admin")]
public class AdminSongReportsController : Controller
{
    private readonly ISongReportService _songReportService;
    private readonly ICurrentUserService _currentUser;
    public AdminSongReportsController(ISongReportService songReportService, ICurrentUserService currentUser)
    {
        _songReportService = songReportService;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index(string? status)
    {
        ViewData["CurrentStatus"] = status ?? "";
        var reports = await _songReportService.GetReportsAsync(status);
        return View(reports);
    }

    [HttpPost]
    [Route("AdminSongReports/Resolve/{id}")]
    public async Task<IActionResult> Resolve(int id, [FromBody] ResolveRequest request)
    {
        var adminId = _currentUser.UserId ?? 0;
        var (success, message) = await _songReportService.ResolveAsync(id, adminId, request.Status);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}

public class ResolveRequest
{
    public string Status { get; set; } = "";
}
