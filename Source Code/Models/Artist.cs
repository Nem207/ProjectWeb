using System.ComponentModel.DataAnnotations;
namespace SpotifyClone.Models;

public class Artist
{
    [Key]
    public int ArtistID { get; set; }
    public string? ArtistName { get; set; }
    public string? Avatar { get; set; }
    public string? CoverImage { get; set; }
    public string? Bio { get; set; }
    public int? UserID { get; set; }
    public bool IsBlocked { get; set; }
    public User? User { get; set; }
    public ICollection<SongArtist> SongArtists { get; set; } = new List<SongArtist>();
    public ICollection<AlbumArtist> AlbumArtists { get; set; } = new List<AlbumArtist>();
    public ICollection<UserFollowArtist> UserFollowArtists { get; set; } = new List<UserFollowArtist>();
    public ArtistStatistic? ArtistStatistic { get; set; }
}