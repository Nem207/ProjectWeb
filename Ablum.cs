using System.ComponentModel.DataAnnotations;
namespace SpotifyClone.Models;
public class Album
{
    [Key]
    public int AlbumID { get; set; }
    public string? AlbumName { get; set; }
    public string? CoverImage { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? AlbumType { get; set; }
    public ICollection<Song> Songs { get; set; } = new List<Song>();
    public ICollection<AlbumArtist> AlbumArtists { get; set; } = new List<AlbumArtist>();
}