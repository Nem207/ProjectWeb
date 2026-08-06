namespace SpotifyClone.Features.MusicPlayer.ViewModels
{
    public class TrendingSongVM
    {
        public int SongId { get; set; }
        public string Title { get; set; } = "";
        public string? CoverImage { get; set; }
        public int? Duration { get; set; }
        public string? AudioUrl { get; set; }
        public List<string> Artists { get; set; } = new();
    }
}