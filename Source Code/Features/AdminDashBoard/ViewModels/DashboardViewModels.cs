namespace SpotifyClone.Features.AdminDashBoard.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalSongs { get; set; }
        public int TotalArtists { get; set; }
        public long TotalStreams { get; set; }
        public List<StreamTrendPoint> StreamTrend { get; set; } = new();
        public List<GenreShare> GenreDistribution { get; set; } = new();
        public List<TopTrack> TopTracks { get; set; } = new();
    }
    public class StreamTrendPoint
    {
        public string Label { get; set; }
        public int TotalPlays { get; set; }
    }
    public class GenreShare
    {
        public string GenreName { get; set; }
        public int SongCount { get; set; }
        public double Percentage { get; set; }
    }
    public class TopTrack
    {
        public int SongId { get; set; }
        public string Title { get; set; }
        public string ArtistName { get; set; }
        public string CoverImageUrl { get; set; }
        public long StreamCount { get; set; }
    }
}
