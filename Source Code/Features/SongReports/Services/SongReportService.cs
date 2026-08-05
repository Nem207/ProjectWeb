using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Features.SongReports.ViewModels;
using SpotifyClone.Models;
namespace SpotifyClone.Features.SongReports.Services;

public class SongReportService : ISongReportService
{
    private static readonly HashSet<string> ValidReasons = new()
    {
        SongReportReason.Copyright,
        SongReportReason.Inappropriate,
        SongReportReason.WrongInfo,
        SongReportReason.PlaybackError,
        SongReportReason.Other
    };

    private readonly SpotifyDbContext _context;
    public SongReportService(SpotifyDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message)> CreateReportAsync(int songId, int? userId, string reason, string? description)
    {
        var songExists = await _context.Songs.AnyAsync(s => s.SongID == songId);
        if (!songExists)
            return (false, "Bài hát không tồn tại.");

        if (string.IsNullOrWhiteSpace(reason) || !ValidReasons.Contains(reason))
            reason = SongReportReason.Other;

        if (description != null && description.Length > 1000)
            description = description.Substring(0, 1000);

        if (userId.HasValue)
        {
            var alreadyReported = await _context.SongReports.AnyAsync(r =>
                r.SongID == songId && r.UserID == userId && r.Status == SongReportStatus.Pending);
            if (alreadyReported)
                return (false, "Bạn đã báo cáo bài hát này rồi, admin đang xử lý.");
        }

        _context.SongReports.Add(new SongReport
        {
            SongID = songId,
            UserID = userId,
            Reason = reason,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Status = SongReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        return (true, "Cảm ơn bạn đã báo cáo. Admin sẽ xem xét sớm nhất.");
    }

    public async Task<List<SongReportListItemVM>> GetReportsAsync(string? status)
    {
        var query = _context.SongReports
            .Include(r => r.Song)!.ThenInclude(s => s.SongArtists)
                .ThenInclude(sa => sa.Artist)
            .Include(r => r.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.Status == status);

        var reports = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reports.Select(r => new SongReportListItemVM
        {
            ReportID = r.ReportID,
            SongID = r.SongID,
            SongTitle = r.Song.Title,
            SongCoverImage = r.Song.CoverImage,
            ArtistNames = string.Join(", ", r.Song.SongArtists.Select(sa => sa.Artist!.ArtistName)),
            UserID = r.UserID,
            ReporterName = r.User?.Username ?? "Ẩn danh",
            Reason = r.Reason,
            Description = r.Description,
            Status = r.Status,
            CreatedAt = r.CreatedAt,
            ReviewedAt = r.ReviewedAt
        }).ToList();
    }

    public async Task<int> CountPendingAsync()
    {
        return await _context.SongReports.CountAsync(r => r.Status == SongReportStatus.Pending);
    }

    public async Task<(bool Success, string Message)> ResolveAsync(int reportId, int adminUserId, string newStatus)
    {
        if (newStatus != SongReportStatus.Reviewed && newStatus != SongReportStatus.Dismissed)
            return (false, "Trạng thái không hợp lệ.");

        var report = await _context.SongReports.FirstOrDefaultAsync(r => r.ReportID == reportId);
        if (report == null)
            return (false, "Không tìm thấy báo cáo.");

        report.Status = newStatus;
        report.ReviewedAt = DateTime.UtcNow;
        report.ReviewedByAdminID = adminUserId;
        await _context.SaveChangesAsync();
        return (true, "Đã cập nhật báo cáo.");
    }
}
