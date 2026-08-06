using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Features.Queue.Services;
using SpotifyClone.Models;

namespace SpotifyClone.Tests.Features.Queue
{
    public class QueueServiceTests
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
        public async Task AddToQueueAsync_ShouldAssignSequentialPositionNumbers()
        {
            
            using var context = CreateContext();
            context.Songs.AddRange(MakeSong(1), MakeSong(2), MakeSong(3));
            await context.SaveChangesAsync();
            var service = new QueueService(context);
            int userId = 1;
            
            var first = await service.AddToQueueAsync(userId, 1);
            var second = await service.AddToQueueAsync(userId, 2);
            var third = await service.AddToQueueAsync(userId, 3);
            
            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.NotNull(third);
            Assert.Equal(1, first!.PositionNumber);
            Assert.Equal(2, second!.PositionNumber);
            Assert.Equal(3, third!.PositionNumber);
        }

        [Fact]
        public async Task AddToQueueAsync_SongDoesNotExist_ReturnsNull()
        {
            
            using var context = CreateContext();
            var service = new QueueService(context);

            
            var result = await service.AddToQueueAsync(userId: 1, songId: 999);

            
            Assert.Null(result);
        }

        [Fact]
        public async Task RemoveFromQueueAsync_RemovingMiddleItem_ShouldShiftLaterPositionsDownByOne()
        {

            using var context = CreateContext();
            context.Songs.AddRange(MakeSong(1), MakeSong(2), MakeSong(3));
            await context.SaveChangesAsync();
            var service = new QueueService(context);
            int userId = 1;
            var item1 = await service.AddToQueueAsync(userId, 1); 
            var item2 = await service.AddToQueueAsync(userId, 2); 
            var item3 = await service.AddToQueueAsync(userId, 3); 

            
            var removed = await service.RemoveFromQueueAsync(userId, item2!.QueueID);
            var remaining = await service.GetQueueAsync(userId);

            
            Assert.True(removed);
            Assert.Equal(2, remaining.Count);
            var remainingItem1 = remaining.Single(q => q.QueueID == item1!.QueueID);
            var remainingItem3 = remaining.Single(q => q.QueueID == item3!.QueueID);
            Assert.Equal(1, remainingItem1.PositionNumber);
            Assert.Equal(2, remainingItem3.PositionNumber); 
        }

        [Fact]
        public async Task RemoveFromQueueAsync_ItemBelongsToDifferentUser_ReturnsFalseAndDoesNotAffectOwner()
        {
            
            using var context = CreateContext();
            context.Songs.Add(MakeSong(1));
            await context.SaveChangesAsync();
            var service = new QueueService(context);
            var ownerItem = await service.AddToQueueAsync(userId: 1, songId: 1);

            
            var removed = await service.RemoveFromQueueAsync(userId: 2, queueId: ownerItem!.QueueID);
            var ownerQueue = await service.GetQueueAsync(userId: 1);

            
            Assert.False(removed);
            Assert.Single(ownerQueue);
        }

        [Fact]
        public async Task ClearQueueAsync_ShouldRemoveOnlyCurrentUsersItems()
        {
            
            using var context = CreateContext();
            context.Songs.AddRange(MakeSong(1), MakeSong(2));
            await context.SaveChangesAsync();
            var service = new QueueService(context);
            await service.AddToQueueAsync(userId: 1, songId: 1);
            await service.AddToQueueAsync(userId: 1, songId: 2);
            await service.AddToQueueAsync(userId: 2, songId: 1);

            await service.ClearQueueAsync(userId: 1);
            
            var user1Queue = await service.GetQueueAsync(userId: 1);
            var user2Queue = await service.GetQueueAsync(userId: 2);
            Assert.Empty(user1Queue);
            Assert.Single(user2Queue);
        }
    }
}
