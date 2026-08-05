using SpotifyClone.Features.Song.ViewModels;
namespace SpotifyClone.Features.Artist.ViewModels
{
    public class ArtistDetailVM
    {
        public int ArtistID { get; set; }
        public string ArtistName { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string? CoverImage { get; set; }
        public string? Bio { get; set; }
        public long MonthlyListeners { get; set; }
        public long TotalFollowers { get; set; }
        public List<SongSummaryVM> PopularSongs { get; set; } = new();
    }
}
