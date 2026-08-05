using Microsoft.EntityFrameworkCore;
using Moq;
using SpotifyClone.Data;
using SpotifyClone.Features.MusicPlayer.Services;
using SpotifyClone.Features.Premium.Services;
using SpotifyClone.Models;

namespace SpotifyClone.Tests.Features.MusicPlayer
{
    public class MusicPlayerServiceTests
    {
        private static SpotifyDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<SpotifyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new SpotifyDbContext(options);
        }

        private static MusicPlayerService CreateService(SpotifyDbContext context, bool isPremium = false)
        {
            var premiumMock = new Mock<IPremiumService>();
            premiumMock.Setup(p => p.HasPremiumAsync(It.IsAny<int>())).ReturnsAsync(isPremium);
            return new MusicPlayerService(context, premiumMock.Object);
        }

        private static Song MakeSong(int id, string title = "Song")
        {
            return new Song { SongID = id, Title = title, Duration = 200, AudioURL = "url", Status = SongStatus.Approved, CreatedAt = DateTime.UtcNow };
        }

        [Fact]
        public async Task IncrementPlayCountAsync_ConsecutivePlaysOfSameSong_ShouldMergeIntoOneHistoryRow()
        {

            using var context = CreateContext();
            context.Songs.Add(MakeSong(1));
            await context.SaveChangesAsync();
            var service = CreateService(context);

            
            await service.IncrementPlayCountAsync(1, userId: 10);
            await service.IncrementPlayCountAsync(1, userId: 10);
            await service.IncrementPlayCountAsync(1, userId: 10);

            
            var history = await context.ListeningHistories.Where(h => h.UserID == 10).ToListAsync();
            Assert.Single(history);

            var song = await context.Songs.Include(s => s.SongStatistic).FirstAsync(s => s.SongID == 1);
            Assert.Equal(3, song.SongStatistic!.TotalPlays);
        }

        [Fact]
        public async Task IncrementPlayCountAsync_DifferentSongsPlayedInSequence_ShouldCreateSeparateHistoryRows()
        {
            
            using var context = CreateContext();
            context.Songs.AddRange(MakeSong(1), MakeSong(2));
            await context.SaveChangesAsync();
            var service = CreateService(context);

            
            await service.IncrementPlayCountAsync(1, userId: 10);
            await service.IncrementPlayCountAsync(2, userId: 10);
            await service.IncrementPlayCountAsync(1, userId: 10);

            
            var history = await context.ListeningHistories.Where(h => h.UserID == 10).ToListAsync();
            Assert.Equal(3, history.Count);
        }

        [Fact]
        public async Task IncrementPlayCountAsync_NoUserId_ShouldOnlyIncrementTotalPlaysWithoutHistory()
        {

            using var context = CreateContext();
            context.Songs.Add(MakeSong(1));
            await context.SaveChangesAsync();
            var service = CreateService(context);

            
            var result = await service.IncrementPlayCountAsync(1, userId: null);

            
            Assert.True(result);
            Assert.Empty(context.ListeningHistories);
            var song = await context.Songs.Include(s => s.SongStatistic).FirstAsync(s => s.SongID == 1);
            Assert.Equal(1, song.SongStatistic!.TotalPlays);
        }

        [Fact]
        public async Task IncrementPlayCountAsync_SongDoesNotExist_ReturnsFalse()
        {
           
            using var context = CreateContext();
            var service = CreateService(context);

            
            var result = await service.IncrementPlayCountAsync(999, userId: 10);

            
            Assert.False(result);
        }
    }
}
