using System.ComponentModel.DataAnnotations;
namespace SpotifyClone.Features.ArtistDashboard.ViewModels;
public class ArtistDashboardViewModel
{
    public int ArtistID { get; set; }
    public string ArtistName { get; set; } = "";
    public string? Avatar { get; set; }
    public int TotalSongs { get; set; }
    public long TotalPlays { get; set; }
    public long TotalLikes { get; set; }
    public long MonthlyListeners { get; set; }
    public long TotalFollowers { get; set; }
    public List<ArtistTopSong> TopSongs { get; set; } = new();
}
public class ArtistTopSong
{
    public int SongId { get; set; }
    public string Title { get; set; } = "";
    public string? CoverImage { get; set; }
    public long TotalPlays { get; set; }
    public long TotalLikes { get; set; }
    public string Status { get; set; } = "";
    public bool IsPremium { get; set; }
}
public class UploadSongViewModel
{
    [Required(ErrorMessage = "Vui long nhap ten bai hat")]
    [StringLength(255)]
    public string Title { get; set; } = "";
    [Required(ErrorMessage = "Vui long nhap URL file nhac hop le de he thong tu lay thoi luong")]
    [Range(1, 7200, ErrorMessage = "Thoi luong khong hop le")]
    public int Duration { get; set; }
    [Required(ErrorMessage = "Vui long nhap URL file nhac")]
    [Url(ErrorMessage = "AudioURL khong hop le")]
    public string AudioURL { get; set; } = "";
    [Url(ErrorMessage = "CoverImage khong hop le")]
    public string? CoverImage { get; set; }
    public bool IsPremium { get; set; }
}
public class ArtistNotificationItem
{
    public int SongId { get; set; }
    public string Title { get; set; } = "";
    public string? CoverImage { get; set; }
    public string Status { get; set; } = "";
    public string? RejectReason { get; set; }
    public DateTime CreatedAt { get; set; }
}