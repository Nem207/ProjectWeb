using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Features.AdminDashBoard.Services;
using SpotifyClone.Models;

namespace SpotifyClone.Tests.Features.AdminDashBoard
{
    
    public class AdminDashboardServiceTests
    {
        private static SpotifyDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<SpotifyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new SpotifyDbContext(options);
        }

        private static Song MakeSong(int id) => new Song
        {
            SongID = id,
            Title = "Song " + id,
            Status = SongStatus.Approved,
            CreatedAt = DateTime.UtcNow
        };

        [Fact]
        public async Task GetDashboardDataAsync_GenreDistribution_ShouldCalculatePercentageRoundedToOneDecimal()
        {
            
            using var context = CreateContext();
            var pop = new Genre { GenreID = 1, GenreName = "Pop" };
            var rock = new Genre { GenreID = 2, GenreName = "Rock" };
            context.Genres.AddRange(pop, rock);
            context.Songs.AddRange(MakeSong(1), MakeSong(2), MakeSong(3), MakeSong(4));
            context.SongGenres.AddRange(
                new SongGenre { SongID = 1, GenreID = 1 },
                new SongGenre { SongID = 2, GenreID = 1 },
                new SongGenre { SongID = 3, GenreID = 1 },
                new SongGenre { SongID = 4, GenreID = 2 });
            await context.SaveChangesAsync();
            var service = new DashboardServiceImpl(context);

            
            var result = await service.GetDashboardDataAsync();

            
            var popShare = result.GenreDistribution.Single(g => g.GenreName == "Pop");
            var rockShare = result.GenreDistribution.Single(g => g.GenreName == "Rock");
            Assert.Equal(75.0, popShare.Percentage);
            Assert.Equal(25.0, rockShare.Percentage);
        }

        [Fact]
        public async Task GetDashboardDataAsync_StreamTrend_ShouldReturnSevenDaysAndFillMissingDaysWithZero()
        {
            
            using var context = CreateContext();
            var user = new User { UserID = 1, Username = "u", Email = "u@test.com", PasswordHash = "x", CreatedAt = DateTime.UtcNow };
            context.Users.Add(user);
            context.Songs.Add(MakeSong(1));
            context.ListeningHistories.Add(new ListeningHistory { UserID = 1, SongID = 1, PlayedAt = DateTime.Now });
            context.ListeningHistories.Add(new ListeningHistory { UserID = 1, SongID = 1, PlayedAt = DateTime.Now });
            await context.SaveChangesAsync();
            var service = new DashboardServiceImpl(context);

            
            var result = await service.GetDashboardDataAsync();

            
            Assert.Equal(7, result.StreamTrend.Count); 
            Assert.Equal(2, result.StreamTrend.Last().TotalPlays); 
            Assert.All(result.StreamTrend.Take(6), point => Assert.Equal(0, point.TotalPlays)); 
        }

        [Fact]
        public async Task GetDashboardDataAsync_TopTracks_ShouldOrderByStreamCountDescendingAndLimitToTen()
        {
            
            using var context = CreateContext();
            var user = new User { UserID = 1, Username = "u", Email = "u@test.com", PasswordHash = "x", CreatedAt = DateTime.UtcNow };
            context.Users.Add(user);
            for (int songId = 1; songId <= 11; songId++)
            {
                context.Songs.Add(MakeSong(songId));
                for (int play = 0; play < songId; play++)
                {
                    context.ListeningHistories.Add(new ListeningHistory { UserID = 1, SongID = songId, PlayedAt = DateTime.Now });
                }
            }
            await context.SaveChangesAsync();
            var service = new DashboardServiceImpl(context);

            
            var result = await service.GetDashboardDataAsync();

            
            Assert.Equal(10, result.TopTracks.Count); 
            Assert.Equal(11, result.TopTracks[0].SongId); 
            Assert.DoesNotContain(result.TopTracks, t => t.SongId == 1); 
        }
    }
}
