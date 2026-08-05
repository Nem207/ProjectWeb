using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Features.AdminSongs.Services;
using SpotifyClone.Models;

namespace SpotifyClone.Tests.Features.AdminSongs
{
    
    public class AdminSongsServiceTests
    {
        private static SpotifyDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<SpotifyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new SpotifyDbContext(options);
        }

        [Fact]
        public async Task BlockSongAsync_ApprovedSong_ShouldBlockSuccessfully()
        {
            
            using var context = CreateContext();
            context.Songs.Add(new Song { SongID = 1, Title = "S", Status = SongStatus.Approved, CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
            var service = new SongsService(context);

            
            var (success, _) = await service.BlockSongAsync(1);

            
            Assert.True(success);
            var song = await context.Songs.FindAsync(1);
            Assert.Equal(SongStatus.Blocked, song!.Status);
        }

        [Fact]
        public async Task BlockSongAsync_SongAlreadyBlocked_ShouldFail()
        {
            
            using var context = CreateContext();
            context.Songs.Add(new Song { SongID = 1, Title = "S", Status = SongStatus.Blocked, CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
            var service = new SongsService(context);

            
            var (success, _) = await service.BlockSongAsync(1);

            
            Assert.False(success);
        }

        [Fact]
        public async Task UnblockSongAsync_BlockedSong_ShouldRestoreToApprovedStatus()
        {

            using var context = CreateContext();
            context.Songs.Add(new Song { SongID = 1, Title = "S", Status = SongStatus.Blocked, CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
            var service = new SongsService(context);

            
            var (success, _) = await service.UnblockSongAsync(1);

            
            Assert.True(success);
            var song = await context.Songs.FindAsync(1);
            Assert.Equal(SongStatus.Approved, song!.Status);
        }

        [Fact]
        public async Task UnblockSongAsync_SongNotBlocked_ShouldFailAndKeepOriginalStatus()
        {
            
            using var context = CreateContext();
            context.Songs.Add(new Song { SongID = 1, Title = "S", Status = SongStatus.Pending, CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
            var service = new SongsService(context);

            
            var (success, _) = await service.UnblockSongAsync(1);

            
            Assert.False(success);
            var song = await context.Songs.FindAsync(1);
            Assert.Equal(SongStatus.Pending, song!.Status); 
        }
    }
}
