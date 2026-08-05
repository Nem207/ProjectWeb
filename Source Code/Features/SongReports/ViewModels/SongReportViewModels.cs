namespace SpotifyClone.Features.SongReports.ViewModels;

public class CreateSongReportRequest
{
    public string Reason { get; set; } = "";
    public string? Description { get; set; }
}

public class SongReportListItemVM
{
    public int ReportID { get; set; }
    public int SongID { get; set; }
    public string SongTitle { get; set; } = "";
    public string? SongCoverImage { get; set; }
    public string ArtistNames { get; set; } = "";
    public int? UserID { get; set; }
    public string ReporterName { get; set; } = "Ẩn danh";
    public string Reason { get; set; } = "";
    public string? Description { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
