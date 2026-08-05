using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Features.AdminArtists.Services;
using SpotifyClone.Models;

namespace SpotifyClone.Tests.Features.AdminArtists
{

    public class AdminArtistsServiceTests
    {
        private static SpotifyDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<SpotifyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new SpotifyDbContext(options);
        }

        [Fact]
        public async Task BlockArtistAsync_ArtistNotBlocked_ShouldBlockSuccessfully()
        {
        
            using var context = CreateContext();
            context.Artists.Add(new Artist { ArtistID = 1, ArtistName = "Artist A", IsBlocked = false });
            await context.SaveChangesAsync();
            var service = new ArtistsService(context);

           
            var (success, _) = await service.BlockArtistAsync(1);

          
            Assert.True(success);
            var artist = await context.Artists.FindAsync(1);
            Assert.True(artist!.IsBlocked);
        }

        [Fact]
        public async Task BlockArtistAsync_ArtistAlreadyBlocked_ShouldFail()
        {
         
            using var context = CreateContext();
            context.Artists.Add(new Artist { ArtistID = 1, ArtistName = "Artist A", IsBlocked = true });
            await context.SaveChangesAsync();
            var service = new ArtistsService(context);

            
            var (success, _) = await service.BlockArtistAsync(1);

           
            Assert.False(success);
        }

        [Fact]
        public async Task UnblockArtistAsync_ArtistIsBlocked_ShouldUnblockSuccessfully()
        {
            
            using var context = CreateContext();
            context.Artists.Add(new Artist { ArtistID = 1, ArtistName = "Artist A", IsBlocked = true });
            await context.SaveChangesAsync();
            var service = new ArtistsService(context);

            
            var (success, _) = await service.UnblockArtistAsync(1);

            
            Assert.True(success);
            var artist = await context.Artists.FindAsync(1);
            Assert.False(artist!.IsBlocked);
        }

        [Fact]
        public async Task UnblockArtistAsync_ArtistNotBlocked_ShouldFail()
        {
            
            using var context = CreateContext();
            context.Artists.Add(new Artist { ArtistID = 1, ArtistName = "Artist A", IsBlocked = false });
            await context.SaveChangesAsync();
            var service = new ArtistsService(context);

            
            var (success, _) = await service.UnblockArtistAsync(1);

            
            Assert.False(success);
        }

        [Fact]
        public async Task BlockArtistAsync_ArtistDoesNotExist_ShouldFail()
        {
            
            using var context = CreateContext();
            var service = new ArtistsService(context);

            
            var (success, _) = await service.BlockArtistAsync(999);

            
            Assert.False(success);
        }
    }
}
