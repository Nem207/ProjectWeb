using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Features.ArtistReports.ViewModels;
using SpotifyClone.Models;
namespace SpotifyClone.Features.ArtistReports.Services;

public class ArtistReportService : IArtistReportService
{
    private static readonly HashSet<string> ValidReasons = new()
    {
        ArtistReportReason.Copyright,
        ArtistReportReason.Inappropriate,
        ArtistReportReason.Impersonation,
        ArtistReportReason.SpamOrScam,
        ArtistReportReason.Other
    };

    private readonly SpotifyDbContext _context;
    public ArtistReportService(SpotifyDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message)> CreateReportAsync(int artistId, int? userId, string reason, string? description)
    {
        var artistExists = await _context.Artists.AnyAsync(a => a.ArtistID == artistId);
        if (!artistExists)
            return (false, "Nghệ sĩ không tồn tại.");

        if (string.IsNullOrWhiteSpace(reason) || !ValidReasons.Contains(reason))
            reason = ArtistReportReason.Other;

        if (description != null && description.Length > 1000)
            description = description.Substring(0, 1000);

        if (userId.HasValue)
        {
            var alreadyReported = await _context.ArtistReports.AnyAsync(r =>
                r.ArtistID == artistId && r.UserID == userId && r.Status == ArtistReportStatus.Pending);
            if (alreadyReported)
                return (false, "Bạn đã báo cáo nghệ sĩ này rồi, admin đang xử lý.");
        }

        _context.ArtistReports.Add(new ArtistReport
        {
            ArtistID = artistId,
            UserID = userId,
            Reason = reason,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Status = ArtistReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        return (true, "Cảm ơn bạn đã báo cáo. Admin sẽ xem xét sớm nhất.");
    }

    public async Task<List<ArtistReportListItemVM>> GetReportsAsync(string? status)
    {
        var query = _context.ArtistReports
            .Include(r => r.Artist)
            .Include(r => r.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.Status == status);

        var reports = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reports.Select(r => new ArtistReportListItemVM
        {
            ReportID = r.ReportID,
            ArtistID = r.ArtistID,
            ArtistName = r.Artist.ArtistName,
            ArtistAvatar = r.Artist.Avatar,
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
        return await _context.ArtistReports.CountAsync(r => r.Status == ArtistReportStatus.Pending);
    }

    public async Task<(bool Success, string Message)> ResolveAsync(int reportId, int adminUserId, string newStatus)
    {
        if (newStatus != ArtistReportStatus.Reviewed && newStatus != ArtistReportStatus.Dismissed)
            return (false, "Trạng thái không hợp lệ.");

        var report = await _context.ArtistReports.FirstOrDefaultAsync(r => r.ReportID == reportId);
        if (report == null)
            return (false, "Không tìm thấy báo cáo.");

        report.Status = newStatus;
        report.ReviewedAt = DateTime.UtcNow;
        report.ReviewedByAdminID = adminUserId;
        await _context.SaveChangesAsync();
        return (true, "Đã cập nhật báo cáo.");
    }
}