using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Features.Search.Services;
using SpotifyClone.Models;

namespace SpotifyClone.Tests.Features.Search
{
    public class SearchServiceTests
    {
        private static SpotifyDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<SpotifyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new SpotifyDbContext(options);
        }

        private static Artist MakeArtist(int id, string name, bool isBlocked = false)
        {
            return new Artist { ArtistID = id, ArtistName = name, IsBlocked = isBlocked };
        }

        private static Song MakeSong(int id, string title, string status = SongStatus.Approved, bool isPremium = false)
        {
            return new Song
            {
                SongID = id,
                Title = title,
                Duration = 200,
                AudioURL = "url",
                Status = status,
                IsPremium = isPremium,
                CreatedAt = DateTime.UtcNow
            };
        }

        [Fact]
        public async Task SearchAsync_ShouldReturnOnlyApprovedSongsMatchingKeyword()
        {
            
            using var context = CreateContext();
            context.Songs.AddRange(
                MakeSong(1, "Hello World", SongStatus.Approved),
                MakeSong(2, "Hello There", SongStatus.Pending),
                MakeSong(3, "Goodbye", SongStatus.Approved));
            await context.SaveChangesAsync();
            var service = new SearchService(context);

            
            var result = await service.SearchAsync("Hello");

            
            Assert.Single(result.Songs);
            Assert.Equal("Hello World", result.Songs[0].Title);
        }

        [Fact]
        public async Task SearchAsync_ShouldExcludeSongsBelongingToBlockedArtist()
        {
            
            using var context = CreateContext();
            var blockedArtist = MakeArtist(1, "Blocked Artist", isBlocked: true);
            var normalArtist = MakeArtist(2, "Normal Artist", isBlocked: false);
            var blockedSong = MakeSong(1, "Song A");
            var normalSong = MakeSong(2, "Song A Two");
            context.Artists.AddRange(blockedArtist, normalArtist);
            context.Songs.AddRange(blockedSong, normalSong);
            context.SongArtists.AddRange(
                new SongArtist { SongID = blockedSong.SongID, ArtistID = blockedArtist.ArtistID },
                new SongArtist { SongID = normalSong.SongID, ArtistID = normalArtist.ArtistID });
            await context.SaveChangesAsync();
            var service = new SearchService(context);

            
            var result = await service.SearchAsync("Song A");

            
            Assert.Single(result.Songs);
            Assert.Equal(normalSong.SongID, result.Songs[0].SongID);
        }

        [Fact]
        public async Task SearchAsync_ShouldOrderResultsByTitleStartingWithKeywordFirst()
        {
            
            using var context = CreateContext();
            context.Songs.AddRange(
                MakeSong(1, "My Rock Song"),
                MakeSong(2, "Rock and Roll"));
            await context.SaveChangesAsync();
            var service = new SearchService(context);

            
            var result = await service.SearchAsync("Rock");

            
            Assert.Equal(2, result.Songs.Count);
            Assert.Equal("Rock and Roll", result.Songs[0].Title);
            Assert.Equal("My Rock Song", result.Songs[1].Title);
        }

        [Fact]
        public async Task SearchAsync_NoMatches_ShouldReturnEmptyLists()
        {
            
            using var context = CreateContext();
            context.Songs.Add(MakeSong(1, "Some Song"));
            await context.SaveChangesAsync();
            var service = new SearchService(context);

            
            var result = await service.SearchAsync("NotExisted");

            
            Assert.Empty(result.Songs);
            Assert.Empty(result.Artists);
            Assert.Empty(result.Albums);
            Assert.Empty(result.Playlists);
        }

        [Fact]
        public async Task SuggestAsync_ShouldReturnAtMostFiveSongsAndFiveArtists()
        {
            
            using var context = CreateContext();
            for (int i = 1; i <= 7; i++)
            {
                context.Songs.Add(MakeSong(i, $"Test Song {i}"));
            }
            for (int i = 1; i <= 7; i++)
            {
                context.Artists.Add(MakeArtist(i, $"Test Artist {i}"));
            }
            await context.SaveChangesAsync();
            var service = new SearchService(context);

            
            var suggestions = await service.SuggestAsync("Test");

            
            Assert.Equal(5, suggestions.Count(s => s.Type == "Song"));
            Assert.Equal(5, suggestions.Count(s => s.Type == "Artist"));
        }

        [Fact]
        public async Task SuggestAsync_ShouldOnlyMatchKeywordAsPrefix()
        {
            
            using var context = CreateContext();
            context.Songs.AddRange(
                MakeSong(1, "Amazing Song"),
                MakeSong(2, "Song is Amazing"));
            await context.SaveChangesAsync();
            var service = new SearchService(context);

            
            var suggestions = await service.SuggestAsync("Amazing");

            
            Assert.Single(suggestions);
            Assert.Equal("Amazing Song", suggestions[0].Name);
        }

        [Fact]
        public async Task GetGenresAsync_ShouldReturnGenresOrderedByName()
        {
            
            using var context = CreateContext();
            context.Genres.AddRange(
                new Genre { GenreID = 1, GenreName = "Rock" },
                new Genre { GenreID = 2, GenreName = "Blues" },
                new Genre { GenreID = 3, GenreName = "Jazz" });
            await context.SaveChangesAsync();
            var service = new SearchService(context);

            
            var genres = await service.GetGenresAsync();

            
            Assert.Equal(3, genres.Count);
            Assert.Equal("Blues", genres[0].GenreName);
            Assert.Equal("Jazz", genres[1].GenreName);
            Assert.Equal("Rock", genres[2].GenreName);
        }

        [Fact]
        public async Task GetGenreAsync_GenreExists_ShouldReturnDetailWithRelatedSongs()
        {
            
            using var context = CreateContext();
            var genre = new Genre { GenreID = 1, GenreName = "Pop" };
            var song = MakeSong(1, "Pop Song");
            context.Genres.Add(genre);
            context.Songs.Add(song);
            context.SongGenres.Add(new SongGenre { SongID = song.SongID, GenreID = genre.GenreID });
            await context.SaveChangesAsync();
            var service = new SearchService(context);

            
            var result = await service.GetGenreAsync(genre.GenreID);

            
            Assert.Equal("Pop", result.GenreName);
            Assert.Single(result.Songs);
            Assert.Equal("Pop Song", result.Songs[0].Title);
        }

        [Fact]
        public async Task GetGenreAsync_GenreDoesNotExist_ShouldThrowException()
        {
            
            using var context = CreateContext();
            var service = new SearchService(context);

            
            await Assert.ThrowsAsync<Exception>(() => service.GetGenreAsync(999));
        }
    }
}
