using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Features.Auth.Services;
using SpotifyClone.Features.ArtistReports.Services;
namespace SpotifyClone.Features.AdminArtistReports.Controllers;

[Authorize(Roles = "Admin")]
public class AdminArtistReportsController : Controller
{
    private readonly IArtistReportService _artistReportService;
    private readonly ICurrentUserService _currentUser;
    public AdminArtistReportsController(IArtistReportService artistReportService, ICurrentUserService currentUser)
    {
        _artistReportService = artistReportService;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index(string? status)
    {
        ViewData["CurrentStatus"] = status ?? "";
        var reports = await _artistReportService.GetReportsAsync(status);
        return View(reports);
    }

    [HttpPost]
    [Route("AdminArtistReports/Resolve/{id}")]
    public async Task<IActionResult> Resolve(int id, [FromBody] ResolveRequest request)
    {
        var adminId = _currentUser.UserId ?? 0;
        var (success, message) = await _artistReportService.ResolveAsync(id, adminId, request.Status);
        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}

public class ResolveRequest
{
    public string Status { get; set; } = "";
}