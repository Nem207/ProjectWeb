namespace SpotifyClone.Features.MusicPlayer.ViewModels
{
    public class HistorySongVM
    {
        public int SongID { get; set; }
        public string Title { get; set; } = "";
        public string? CoverImage { get; set; }
        public int Duration { get; set; }
        public string? AudioUrl { get; set; }
        public bool IsPremium { get; set; }
        public string ArtistNames { get; set; } = "";
        public int? MainArtistID { get; set; }
        public DateTime PlayedAt { get; set; }
    }
}