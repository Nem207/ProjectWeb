using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Features.LikedSongs.Services;
using SpotifyClone.Models;

namespace SpotifyClone.Tests.Features.LikedSongs
{
    public class LikedSongsServiceTests
    {
        private static SpotifyDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<SpotifyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new SpotifyDbContext(options);
        }

        private static Song MakeSong(int id, string title = "Song")
        {
            return new Song { SongID = id, Title = title, Duration = 200, AudioURL = "url", Status = SongStatus.Approved, CreatedAt = DateTime.UtcNow };
        }

        [Fact]
        public async Task ToggleLikeAsync_SongNotYetLiked_ShouldLikeAndReturnTrue()
        {
            
            using var context = CreateContext();
            context.Songs.Add(MakeSong(1));
            await context.SaveChangesAsync();
            var service = new LikedSongsService(context);

            
            var result = await service.ToggleLikeAsync(userId: 1, songId: 1);

            
            Assert.True(result);
            var liked = await context.UserLikedSongs.SingleAsync(x => x.UserID == 1 && x.SongID == 1);
            Assert.NotNull(liked);
        }

        [Fact]
        public async Task ToggleLikeAsync_SongAlreadyLiked_ShouldUnlikeAndReturnFalse()
        {
            
            using var context = CreateContext();
            context.Songs.Add(MakeSong(1));
            await context.SaveChangesAsync();
            var service = new LikedSongsService(context);
            await service.ToggleLikeAsync(userId: 1, songId: 1); 

            
            var result = await service.ToggleLikeAsync(userId: 1, songId: 1); 

            
            Assert.False(result);
            Assert.False(await context.UserLikedSongs.AnyAsync(x => x.UserID == 1 && x.SongID == 1));
        }

        [Fact]
        public async Task IsLikedAsync_ShouldReflectCurrentLikeState()
        {
            
            using var context = CreateContext();
            context.Songs.Add(MakeSong(1));
            await context.SaveChangesAsync();
            var service = new LikedSongsService(context);

            
            Assert.False(await service.IsLikedAsync(userId: 1, songId: 1));

            await service.ToggleLikeAsync(userId: 1, songId: 1);
            Assert.True(await service.IsLikedAsync(userId: 1, songId: 1));

            await service.ToggleLikeAsync(userId: 1, songId: 1);
            Assert.False(await service.IsLikedAsync(userId: 1, songId: 1));
        }

        [Fact]
        public async Task ToggleLikeAsync_DifferentUsersLikingSameSong_ShouldNotAffectEachOther()
        {
            
            using var context = CreateContext();
            context.Songs.Add(MakeSong(1));
            await context.SaveChangesAsync();
            var service = new LikedSongsService(context);

            
            await service.ToggleLikeAsync(userId: 1, songId: 1);

            
            Assert.True(await service.IsLikedAsync(userId: 1, songId: 1));
            Assert.False(await service.IsLikedAsync(userId: 2, songId: 1));
        }
    }
}
