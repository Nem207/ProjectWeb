using Microsoft.EntityFrameworkCore;
using SpotifyClone.Models;
namespace SpotifyClone.Data
{
    public class SpotifyDbContext : DbContext
    {
        public SpotifyDbContext(DbContextOptions<SpotifyDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Lyric> Lyrics { get; set; }
        public DbSet<SongQuality> SongQualities { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<PlaylistSong> PlaylistSongs { get; set; }
        public DbSet<PlaylistMember> PlaylistMembers { get; set; }
        public DbSet<SongArtist> SongArtists { get; set; }
        public DbSet<AlbumArtist> AlbumArtists { get; set; }
        public DbSet<SongGenre> SongGenres { get; set; }
        public DbSet<ListeningHistory> ListeningHistories { get; set; }
        public DbSet<SearchHistory> SearchHistories { get; set; }
        public DbSet<UserQueue> UserQueues { get; set; }
        public DbSet<Download> Downloads { get; set; }
        public DbSet<PremiumPlan> PremiumPlans { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserLikedSong> UserLikedSongs { get; set; }
        public DbSet<UserFollowArtist> UserFollowArtists { get; set; }
        public DbSet<BlockedArtist> BlockedArtists { get; set; }
        public DbSet<SongStatistic> SongStatistics { get; set; }
        public DbSet<ArtistStatistic> ArtistStatistics { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<SongReport> SongReports { get; set; }
        public DbSet<ArtistReport> ArtistReports { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SongArtist>()
                .HasKey(x => new { x.SongID, x.ArtistID });
            modelBuilder.Entity<AlbumArtist>()
                .HasKey(x => new { x.AlbumID, x.ArtistID });
            modelBuilder.Entity<SongGenre>()
                .HasKey(x => new { x.SongID, x.GenreID });
            modelBuilder.Entity<PlaylistSong>()
                .HasKey(x => new { x.PlaylistID, x.SongID });
            modelBuilder.Entity<PlaylistMember>()
                .HasKey(x => new { x.PlaylistID, x.UserID });
            modelBuilder.Entity<UserLikedSong>()
                .HasKey(x => new { x.UserID, x.SongID });
            modelBuilder.Entity<UserFollowArtist>()
                .HasKey(x => new { x.UserID, x.ArtistID });
            modelBuilder.Entity<BlockedArtist>()
                .HasKey(x => new { x.UserID, x.ArtistID });
            modelBuilder.Entity<SongArtist>()
                .HasOne(x => x.Song)
                .WithMany(x => x.SongArtists)
                .HasForeignKey(x => x.SongID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SongArtist>()
                .HasOne(x => x.Artist)
                .WithMany(x => x.SongArtists)
                .HasForeignKey(x => x.ArtistID);
            modelBuilder.Entity<AlbumArtist>()
                .HasOne(x => x.Album)
                .WithMany(x => x.AlbumArtists)
                .HasForeignKey(x => x.AlbumID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<AlbumArtist>()
                .HasOne(x => x.Artist)
                .WithMany(x => x.AlbumArtists)
                .HasForeignKey(x => x.ArtistID);
            modelBuilder.Entity<SongGenre>()
                .HasOne(x => x.Song)
                .WithMany(x => x.SongGenres)
                .HasForeignKey(x => x.SongID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SongGenre>()
                .HasOne(x => x.Genre)
                .WithMany(x => x.SongGenres)
                .HasForeignKey(x => x.GenreID);
            modelBuilder.Entity<PlaylistSong>()
                .HasOne(x => x.Playlist)
                .WithMany(x => x.PlaylistSongs)
                .HasForeignKey(x => x.PlaylistID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PlaylistSong>()
                .HasOne(x => x.Song)
                .WithMany(x => x.PlaylistSongs)
                .HasForeignKey(x => x.SongID);
            modelBuilder.Entity<PlaylistMember>()
                .HasOne(x => x.Playlist)
                .WithMany(x => x.PlaylistMembers)
                .HasForeignKey(x => x.PlaylistID);
            modelBuilder.Entity<PlaylistMember>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserID);
            modelBuilder.Entity<UserLikedSong>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserLikedSongs)
                .HasForeignKey(x => x.UserID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<UserLikedSong>()
                .HasOne(x => x.Song)
                .WithMany(x => x.UserLikedSongs)
                .HasForeignKey(x => x.SongID);
            modelBuilder.Entity<UserFollowArtist>()
                .ToTable("UserFollowsArtists");
            modelBuilder.Entity<UserFollowArtist>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserFollowArtists)
                .HasForeignKey(x => x.UserID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<UserFollowArtist>()
                .HasOne(x => x.Artist)
                .WithMany(x => x.UserFollowArtists)
                .HasForeignKey(x => x.ArtistID);
            modelBuilder.Entity<BlockedArtist>()
                .ToTable("BlockedArtists");
            modelBuilder.Entity<BlockedArtist>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<BlockedArtist>()
                .HasOne(x => x.Artist)
                .WithMany()
                .HasForeignKey(x => x.ArtistID);
            modelBuilder.Entity<Song>()
                .HasOne(x => x.Album)
                .WithMany(x => x.Songs)
                .HasForeignKey(x => x.AlbumID)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Playlist>()
                .HasOne(x => x.User)
                .WithMany(x => x.Playlists)
                .HasForeignKey(x => x.UserID);
            modelBuilder.Entity<ListeningHistory>()
                .ToTable("ListeningHistory");
            modelBuilder.Entity<ListeningHistory>()
                .HasOne(x => x.User)
                .WithMany(x => x.ListeningHistories)
                .HasForeignKey(x => x.UserID);
            modelBuilder.Entity<ListeningHistory>()
                .HasOne(x => x.Song)
                .WithMany(x => x.ListeningHistories)
                .HasForeignKey(x => x.SongID);
            modelBuilder.Entity<SearchHistory>()
                .ToTable("SearchHistory");
            modelBuilder.Entity<SearchHistory>()
                .HasOne(x => x.User)
                .WithMany(x => x.SearchHistories)
                .HasForeignKey(x => x.UserID);
            modelBuilder.Entity<UserQueue>()
                .ToTable("UserQueue");
            modelBuilder.Entity<UserQueue>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserID);
            modelBuilder.Entity<UserQueue>()
                .HasOne(x => x.Song)
                .WithMany()
                .HasForeignKey(x => x.SongID);
            modelBuilder.Entity<UserQueue>()
                .HasIndex(x => new { x.UserID, x.PositionNumber })
                .IsUnique();
            modelBuilder.Entity<Download>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserID);
            modelBuilder.Entity<Download>()
                .HasOne(x => x.Song)
                .WithMany(x => x.Downloads)
                .HasForeignKey(x => x.SongID);
            modelBuilder.Entity<SongQuality>()
                .HasOne(x => x.Song)
                .WithMany(x => x.SongQualities)
                .HasForeignKey(x => x.SongID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SongQuality>()
                .HasIndex(x => new { x.SongID, x.QualityName })
                .IsUnique();
            modelBuilder.Entity<Lyric>()
                .HasOne(x => x.Song)
                .WithOne(x => x.Lyric)
                .HasForeignKey<Lyric>(x => x.SongID);
            modelBuilder.Entity<SongStatistic>()
                .HasOne(x => x.Song)
                .WithOne(x => x.SongStatistic)
                .HasForeignKey<SongStatistic>(x => x.SongID);
            modelBuilder.Entity<Artist>()
                .HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<Artist>(x => x.UserID)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<ArtistStatistic>()
                .HasOne(x => x.Artist)
                .WithOne(x => x.ArtistStatistic)
                .HasForeignKey<ArtistStatistic>(x => x.ArtistID);
            modelBuilder.Entity<UserPreference>()
                .HasOne(x => x.User)
                .WithOne(x => x.UserPreference)
                .HasForeignKey<UserPreference>(x => x.UserID);
            modelBuilder.Entity<UserSubscription>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserSubscriptions)
                .HasForeignKey(x => x.UserID);
            modelBuilder.Entity<UserSubscription>()
                .HasOne(x => x.PremiumPlan)
                .WithMany(x => x.UserSubscriptions)
                .HasForeignKey(x => x.PlanID);
            modelBuilder.Entity<Payment>()
                .HasOne(x => x.User)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.UserID);
            modelBuilder.Entity<Payment>()
                .HasOne(x => x.UserSubscription)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.SubscriptionID);
            modelBuilder.Entity<Notification>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserID);
            modelBuilder.Entity<SongReport>()
                .HasOne(x => x.Song)
                .WithMany()
                .HasForeignKey(x => x.SongID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SongReport>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserID)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<SongReport>()
                .HasOne(x => x.ReviewedByAdmin)
                .WithMany()
                .HasForeignKey(x => x.ReviewedByAdminID)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<SongReport>()
                .HasIndex(x => new { x.SongID, x.UserID, x.Status });
            modelBuilder.Entity<ArtistReport>()
                .HasOne(x => x.Artist)
                .WithMany()
                .HasForeignKey(x => x.ArtistID)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ArtistReport>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserID)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<ArtistReport>()
                .HasOne(x => x.ReviewedByAdmin)
                .WithMany()
                .HasForeignKey(x => x.ReviewedByAdminID)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<ArtistReport>()
                .HasIndex(x => new { x.ArtistID, x.UserID, x.Status });
        }
    }
}