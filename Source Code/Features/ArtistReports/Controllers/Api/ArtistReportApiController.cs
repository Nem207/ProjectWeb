using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Features.Auth.Services;
using SpotifyClone.Features.ArtistReports.Services;
using SpotifyClone.Features.ArtistReports.ViewModels;
namespace SpotifyClone.Features.ArtistReports.Controllers.Api;

[Route("api/artist-report")]
[ApiController]
public class ArtistReportApiController : ControllerBase
{
    private readonly IArtistReportService _artistReportService;
    private readonly ICurrentUserService _currentUser;
    public ArtistReportApiController(IArtistReportService artistReportService, ICurrentUserService currentUser)
    {
        _artistReportService = artistReportService;
        _currentUser = currentUser;
    }

    [HttpPost("{artistId}")]
    public async Task<IActionResult> Report(int artistId, [FromBody] CreateArtistReportRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new { message = "Vui lòng chọn lý do báo cáo." });

        var (success, message) = await _artistReportService.CreateReportAsync(
            artistId, _currentUser.UserId, request.Reason, request.Description);

        if (!success) return BadRequest(new { message });
        return Ok(new { message });
    }
}