using SpotifyClone.Features.ArtistReports.ViewModels;

namespace SpotifyClone.Features.ArtistReports.Services;

public interface IArtistReportService
{
    Task<(bool Success, string Message)> CreateReportAsync(int artistId, int? userId, string reason, string? description);
    Task<List<ArtistReportListItemVM>> GetReportsAsync(string? status);
    Task<int> CountPendingAsync();
    Task<(bool Success, string Message)> ResolveAsync(int reportId, int adminUserId, string newStatus);
}