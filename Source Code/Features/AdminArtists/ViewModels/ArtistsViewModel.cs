namespace SpotifyClone.Features.AdminArtists.ViewModels
{
    public class ArtistsViewModel
    {
        public int ArtistID { get; set; }
        public string ArtistName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsBlocked { get; set; }
    }
}