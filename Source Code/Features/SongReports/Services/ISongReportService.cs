using SpotifyClone.Features.SongReports.ViewModels;
namespace SpotifyClone.Features.SongReports.Services;

public interface ISongReportService
{
    Task<(bool Success, string Message)> CreateReportAsync(int songId, int? userId, string reason, string? description);
    Task<List<SongReportListItemVM>> GetReportsAsync(string? status);
    Task<int> CountPendingAsync();
    Task<(bool Success, string Message)> ResolveAsync(int reportId, int adminUserId, string newStatus);
}
