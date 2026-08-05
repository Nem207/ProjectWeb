namespace SpotifyClone.Features.AdminAlbums.ViewModels
{
    public class AlbumsViewModel
    {
        public int AlbumID { get; set; }
        public List<int> ArtistIDs { get; set; } = new List<int>();
        public string? ArtistName { get; set; }
        public string AlbumTitle { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }
}
