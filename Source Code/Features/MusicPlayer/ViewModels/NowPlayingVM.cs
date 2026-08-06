namespace SpotifyClone.Features.MusicPlayer.ViewModels
{
    public class NowPlayingVM
    {
        public int SongID { get; set; }
        public string Title { get; set; } = "";
        public string? CoverImage { get; set; }
        public int? Duration { get; set; }
        public bool IsPremium { get; set; }
        public int? MaxPreviewSeconds { get; set; }
        public string? AlbumName { get; set; }
        public string ArtistNames { get; set; } = "";
        public int? MainArtistID { get; set; }
        public string? AudioUrl { get; set; }
        public long TotalPlays { get; set; }
        public List<SongQualityVM> Qualities { get; set; } = new();
    }
}