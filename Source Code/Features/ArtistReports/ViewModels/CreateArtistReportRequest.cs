namespace SpotifyClone.Features.ArtistReports.ViewModels;

public class CreateArtistReportRequest
{
    public string Reason { get; set; } = "";
    public string? Description { get; set; }
}