using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Features.AdminSongApproval.Services;
using SpotifyClone.Models;

namespace SpotifyClone.Tests.Features.AdminSongApproval
{
    
    public class AdminSongApprovalServiceTests
    {
        private static SpotifyDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<SpotifyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new SpotifyDbContext(options);
        }

        private static Song MakePendingSong(int id)
        {
            return new Song { SongID = id, Title = "Song " + id, Status = SongStatus.Pending, CreatedAt = DateTime.UtcNow };
        }

        [Fact]
        public async Task ApproveSongAsync_PendingSong_ShouldApproveAndClearRejectReason()
        {
            
            using var context = CreateContext();
            var song = MakePendingSong(1);
            song.RejectReason = "Lý do từ chối lần trước";
            context.Songs.Add(song);
            await context.SaveChangesAsync();
            var service = new SongApprovalService(context);

            
            var (success, message) = await service.ApproveSongAsync(1);

            
            Assert.True(success);
            var updated = await context.Songs.FindAsync(1);
            Assert.Equal(SongStatus.Approved, updated!.Status);
            Assert.Null(updated.RejectReason); 
        }

        [Fact]
        public async Task ApproveSongAsync_SongAlreadyProcessed_ShouldFailAndNotChangeStatus()
        {
            
            using var context = CreateContext();
            context.Songs.Add(new Song { SongID = 1, Title = "S", Status = SongStatus.Approved, CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
            var service = new SongApprovalService(context);

            
            var (success, _) = await service.ApproveSongAsync(1);

            
            Assert.False(success);
            var song = await context.Songs.FindAsync(1);
            Assert.Equal(SongStatus.Approved, song!.Status); 
        }

        [Fact]
        public async Task RejectSongAsync_PendingSongWithReason_ShouldRejectAndStoreReason()
        {
            
            using var context = CreateContext();
            context.Songs.Add(MakePendingSong(1));
            await context.SaveChangesAsync();
            var service = new SongApprovalService(context);

            
            var (success, _) = await service.RejectSongAsync(1, "Vi phạm bản quyền");

            
            Assert.True(success);
            var song = await context.Songs.FindAsync(1);
            Assert.Equal(SongStatus.Rejected, song!.Status);
            Assert.Equal("Vi phạm bản quyền", song.RejectReason);
        }

        [Fact]
        public async Task RejectSongAsync_MissingReason_ShouldFailAndKeepSongPending()
        {
            
            using var context = CreateContext();
            context.Songs.Add(MakePendingSong(1));
            await context.SaveChangesAsync();
            var service = new SongApprovalService(context);

            
            var (success, _) = await service.RejectSongAsync(1, reason: "   ");

            
            Assert.False(success);
            var song = await context.Songs.FindAsync(1);
            Assert.Equal(SongStatus.Pending, song!.Status); 
        }

        [Fact]
        public async Task ApproveSongAsync_SongDoesNotExist_ShouldFail()
        {
            
            using var context = CreateContext();
            var service = new SongApprovalService(context);

            
            var (success, _) = await service.ApproveSongAsync(999);

            
            Assert.False(success);
        }
    }
}
