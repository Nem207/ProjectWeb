using SpotifyClone.Features.Song.ViewModels;
namespace SpotifyClone.Features.Album.ViewModels
{
    public class AlbumDetailVM
    {
        public int AlbumID { get; set; }
        public string AlbumName { get; set; } = string.Empty;
        public string? CoverImage { get; set; }
        public string? AlbumType { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int MainArtistID { get; set; }
        public string MainArtistName { get; set; } = string.Empty;
        public string? MainArtistAvatar { get; set; }
        public List<AlbumSongVM> Songs { get; set; } = new();
        public List<AlbumSummaryVM> OtherAlbums { get; set; } = new();
        public int TotalDurationSeconds => Songs.Sum(s => s.Duration);
    }
    public class AlbumSongVM
    {
        public int SongID { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Duration { get; set; }
        public string? CoverImage { get; set; }
        public string? AudioURL { get; set; }
        public List<ArtistLinkViewModel> Artists { get; set; } = new();
    }
    public class AlbumSummaryVM
    {
        public int AlbumID { get; set; }
        public string AlbumName { get; set; } = string.Empty;
        public string? CoverImage { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? AlbumType { get; set; }
    }
}