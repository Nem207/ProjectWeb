namespace SpotifyClone.Features.ArtistReports.ViewModels;

public class ArtistReportListItemVM
{
    public int ReportID { get; set; }
    public int ArtistID { get; set; }
    public string ArtistName { get; set; } = "";
    public string? ArtistAvatar { get; set; }
    public int? UserID { get; set; }
    public string ReporterName { get; set; } = "Ẩn danh";
    public string Reason { get; set; } = "";
    public string? Description { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}