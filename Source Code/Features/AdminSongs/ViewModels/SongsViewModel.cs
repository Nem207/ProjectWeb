namespace SpotifyClone.Features.AdminSongs.ViewModels
{
    public class SongsViewModel
    {
        public int SongID { get; set; }
        public string? Title { get; set; }
        public int Duration { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ArtistName { get; set; } = "Chưa có nghệ sĩ";
        public bool IsPremium { get; set; }
        public string Status { get; set; } = "";
    }
}