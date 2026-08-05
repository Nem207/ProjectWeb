using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Features.Auth.Services;
using SpotifyClone.Features.SongReports.Services;
using SpotifyClone.Features.SongReports.ViewModels;
namespace SpotifyClone.Features.SongReports.Controllers.Api;

[Route("api/song-report")]
[ApiController]
public class SongReportApiController : ControllerBase
{
    private readonly ISongReportService _songReportService;
    private readonly ICurrentUserService _currentUser;
    public SongReportApiController(ISongReportService songReportService, ICurrentUserService currentUser)
    {
        _songReportService = songReportService;
        _currentUser = currentUser;
    }

    [HttpPost("{songId}")]
    public async Task<IActionResult> Report(int songId, [FromBody] CreateSongReportRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { message = "Vui lòng chọn lý do báo cáo." });

        var (success, message) = await _songReportService.CreateReportAsync(
            songId, _currentUser.UserId, request.Reason, request.Description);

        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}
