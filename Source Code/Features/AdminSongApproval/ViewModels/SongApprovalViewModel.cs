namespace SpotifyClone.Features.AdminSongApproval.ViewModels
{
    public class SongApprovalViewModel
    {
        public int SongID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? CoverImage { get; set; }
        public string? AudioURL { get; set; }
        public int Duration { get; set; }
        public string ArtistName { get; set; } = "Chưa rõ";
        public string? AlbumTitle { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "Pending";
        public string? RejectReason { get; set; }
        public bool IsPremium { get; set; }
    }
}