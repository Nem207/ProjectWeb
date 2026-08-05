using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SpotifyClone.Data;
using SpotifyClone.Features.Premium.Services;
using SpotifyClone.Features.Premium.ViewModels;
using SpotifyClone.Models;

namespace SpotifyClone.Tests.Features.Premium
{
    public class PremiumServiceTests
    {

        private static SpotifyDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<SpotifyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new SpotifyDbContext(options);
        }

        private static User MakeUser(int id, string username = "user")
        {
            return new User
            {
                UserID = id,
                Username = username,
                Email = $"{username}@test.com",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow
            };
        }

        private static PremiumPlan MakePlan(int id, string name, decimal price, int durationDays)
        {
            return new PremiumPlan { PlanID = id, PlanName = name, Price = price, DurationDays = durationDays };
        }

        [Fact]
        public async Task GetPlansAsync_ShouldReturnPlansOrderedByPriceAscending()
        {
            
            using var context = CreateContext();
            context.PremiumPlans.AddRange(
                MakePlan(1, "Premium", 99000, 30),
                MakePlan(2, "Free Trial", 0, 7),
                MakePlan(3, "Family", 149000, 30));
            await context.SaveChangesAsync();
            var service = new PremiumService(context);

            
            var plans = await service.GetPlansAsync();

            
            Assert.Equal(3, plans.Count);
            Assert.Equal("Free Trial", plans[0].PlanName);
            Assert.Equal("Premium", plans[1].PlanName);
            Assert.Equal("Family", plans[2].PlanName);
        }

        [Fact]
        public async Task SubscribeAsync_NewUser_ShouldCreateSubscriptionAndPayment()
        {
            
            using var context = CreateContext();
            var user = MakeUser(1);
            var plan = MakePlan(1, "Premium", 99000, 30);
            context.Users.Add(user);
            context.PremiumPlans.Add(plan);
            await context.SaveChangesAsync();
            var service = new PremiumService(context);
            var model = new SubscribeVM { UserID = user.UserID, PlanID = plan.PlanID, PaymentMethod = "CreditCard" };

            
            var result = await service.SubscribeAsync(model);

            
            Assert.True(result);
            var subscription = await context.UserSubscriptions.SingleAsync(x => x.UserID == user.UserID);
            Assert.Equal("Active", subscription.Status);
            var payment = await context.Payments.SingleAsync(x => x.UserID == user.UserID);
            Assert.Equal(plan.Price, payment.Amount);
            Assert.Equal("CreditCard", payment.PaymentMethod);
        }

        [Fact]
        public async Task SubscribeAsync_PlanDoesNotExist_ShouldReturnFalse()
        {
            
            using var context = CreateContext();
            var user = MakeUser(1);
            context.Users.Add(user);
            await context.SaveChangesAsync();
            var service = new PremiumService(context);
            var model = new SubscribeVM { UserID = user.UserID, PlanID = 999, PaymentMethod = "CreditCard" };

            
            var result = await service.SubscribeAsync(model);

            
            Assert.False(result);
        }

        [Fact]
        public async Task SubscribeAsync_UserDoesNotExist_ShouldReturnFalse()
        {
            
            using var context = CreateContext();
            var plan = MakePlan(1, "Premium", 99000, 30);
            context.PremiumPlans.Add(plan);
            await context.SaveChangesAsync();
            var service = new PremiumService(context);
            var model = new SubscribeVM { UserID = 999, PlanID = plan.PlanID, PaymentMethod = "CreditCard" };

            
            var result = await service.SubscribeAsync(model);

            
            Assert.False(result);
        }

        [Fact]
        public async Task SubscribeAsync_UserAlreadyHasActiveSubscription_ShouldExtendEndDateInsteadOfCreatingNew()
        {
 
            using var context = CreateContext();
            var user = MakeUser(1);
            var plan = MakePlan(1, "Premium", 99000, 30);
            context.Users.Add(user);
            context.PremiumPlans.Add(plan);
            await context.SaveChangesAsync();
            var existingSubscription = new UserSubscription
            {
                UserID = user.UserID,
                PlanID = plan.PlanID,
                StartDate = DateTime.Now.AddDays(-5),
                EndDate = DateTime.Now.AddDays(10),
                Status = "Active"
            };
            context.UserSubscriptions.Add(existingSubscription);
            await context.SaveChangesAsync();
            var expectedEndDate = existingSubscription.EndDate.AddDays(plan.DurationDays);
            var service = new PremiumService(context);
            var model = new SubscribeVM { UserID = user.UserID, PlanID = plan.PlanID, PaymentMethod = "CreditCard" };

            
            var result = await service.SubscribeAsync(model);

            
            Assert.True(result);
            var subscriptions = await context.UserSubscriptions.Where(x => x.UserID == user.UserID).ToListAsync();
            Assert.Single(subscriptions);
            Assert.Equal(expectedEndDate.Date, subscriptions[0].EndDate.Date);
        }

        [Fact]
        public async Task CancelSubscriptionAsync_ActiveSubscriptionExists_ShouldSetStatusToCancelled()
        {
            
            using var context = CreateContext();
            var user = MakeUser(1);
            var plan = MakePlan(1, "Premium", 99000, 30);
            context.Users.Add(user);
            context.PremiumPlans.Add(plan);
            var subscription = new UserSubscription
            {
                UserID = user.UserID,
                PlanID = plan.PlanID,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                Status = "Active"
            };
            context.UserSubscriptions.Add(subscription);
            await context.SaveChangesAsync();
            var service = new PremiumService(context);

            
            var result = await service.CancelSubscriptionAsync(new CancelSubscriptionVM { UserID = user.UserID, Reason = "Test" });

            
            Assert.True(result);
            var updated = await context.UserSubscriptions.SingleAsync(x => x.SubscriptionID == subscription.SubscriptionID);
            Assert.Equal("Cancelled", updated.Status);
        }

        [Fact]
        public async Task CancelSubscriptionAsync_NoActiveSubscription_ShouldReturnFalse()
        {
            
            using var context = CreateContext();
            var user = MakeUser(1);
            context.Users.Add(user);
            await context.SaveChangesAsync();
            var service = new PremiumService(context);

            
            var result = await service.CancelSubscriptionAsync(new CancelSubscriptionVM { UserID = user.UserID });

            
            Assert.False(result);
        }

        [Fact]
        public async Task CheckPremiumAsync_NoSubscription_ShouldReturnStatusNone()
        {
            
            using var context = CreateContext();
            var service = new PremiumService(context);

            
            var status = await service.CheckPremiumAsync(userId: 1);

            
            Assert.False(status.IsPremium);
            Assert.Equal("None", status.Status);
        }

        [Fact]
        public async Task CheckPremiumAsync_ActiveAndNotExpired_ShouldReturnIsPremiumTrue()
        {
            
            using var context = CreateContext();
            var user = MakeUser(1);
            var plan = MakePlan(1, "Premium", 99000, 30);
            context.Users.Add(user);
            context.PremiumPlans.Add(plan);
            context.UserSubscriptions.Add(new UserSubscription
            {
                UserID = user.UserID,
                PlanID = plan.PlanID,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(10),
                Status = "Active"
            });
            await context.SaveChangesAsync();
            var service = new PremiumService(context);

            
            var status = await service.CheckPremiumAsync(user.UserID);

            
            Assert.True(status.IsPremium);
            Assert.Equal("Active", status.Status);
        }

        [Fact]
        public async Task CheckPremiumAsync_ActiveButPastEndDate_ShouldMarkExpiredAndPersist()
        {

            using var context = CreateContext();
            var user = MakeUser(1);
            var plan = MakePlan(1, "Premium", 99000, 30);
            context.Users.Add(user);
            context.PremiumPlans.Add(plan);
            var subscription = new UserSubscription
            {
                UserID = user.UserID,
                PlanID = plan.PlanID,
                StartDate = DateTime.Now.AddDays(-40),
                EndDate = DateTime.Now.AddDays(-10),
                Status = "Active"
            };
            context.UserSubscriptions.Add(subscription);
            await context.SaveChangesAsync();
            var service = new PremiumService(context);

            
            var status = await service.CheckPremiumAsync(user.UserID);

            
            Assert.False(status.IsPremium);
            Assert.Equal("Expired", status.Status);
            var updated = await context.UserSubscriptions.SingleAsync(x => x.SubscriptionID == subscription.SubscriptionID);
            Assert.Equal("Expired", updated.Status);
        }

        [Fact]
        public async Task CheckPremiumAsync_StatusNotActive_ShouldReturnIsPremiumFalseWithSameStatus()
        {
            
            using var context = CreateContext();
            var user = MakeUser(1);
            var plan = MakePlan(1, "Premium", 99000, 30);
            context.Users.Add(user);
            context.PremiumPlans.Add(plan);
            context.UserSubscriptions.Add(new UserSubscription
            {
                UserID = user.UserID,
                PlanID = plan.PlanID,
                StartDate = DateTime.Now.AddDays(-10),
                EndDate = DateTime.Now.AddDays(20),
                Status = "Cancelled"
            });
            await context.SaveChangesAsync();
            var service = new PremiumService(context);

            
            var status = await service.CheckPremiumAsync(user.UserID);

            
            Assert.False(status.IsPremium);
            Assert.Equal("Cancelled", status.Status);
        }

        [Fact]
        public async Task HasPremiumAsync_ShouldReflectCheckPremiumResult()
        {
            
            using var context = CreateContext();
            var user = MakeUser(1);
            var plan = MakePlan(1, "Premium", 99000, 30);
            context.Users.Add(user);
            context.PremiumPlans.Add(plan);
            context.UserSubscriptions.Add(new UserSubscription
            {
                UserID = user.UserID,
                PlanID = plan.PlanID,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(10),
                Status = "Active"
            });
            await context.SaveChangesAsync();
            var service = new PremiumService(context);

            
            var hasPremium = await service.HasPremiumAsync(user.UserID);

            
            Assert.True(hasPremium);
        }

        [Fact]
        public async Task GetPaymentHistoryAsync_ShouldReturnPaymentsOrderedByDateDescending()
        {
            
            using var context = CreateContext();
            var user = MakeUser(1);
            var plan = MakePlan(1, "Premium", 99000, 30);
            context.Users.Add(user);
            context.PremiumPlans.Add(plan);
            var subscription = new UserSubscription
            {
                UserID = user.UserID,
                PlanID = plan.PlanID,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                Status = "Active"
            };
            context.UserSubscriptions.Add(subscription);
            await context.SaveChangesAsync();
            context.Payments.AddRange(
                new Payment { UserID = user.UserID, SubscriptionID = subscription.SubscriptionID, Amount = 99000, PaymentMethod = "Card", PaymentDate = DateTime.Now.AddDays(-10) },
                new Payment { UserID = user.UserID, SubscriptionID = subscription.SubscriptionID, Amount = 99000, PaymentMethod = "Card", PaymentDate = DateTime.Now });
            await context.SaveChangesAsync();
            var service = new PremiumService(context);

            
            var history = await service.GetPaymentHistoryAsync(user.UserID);

            
            Assert.Equal(2, history.Count);
            Assert.True(history[0].PaymentDate > history[1].PaymentDate);
        }

        [Fact]
        public async Task GetRevenueAsync_ShouldReturnCorrectTotals()
        {
            
            using var context = CreateContext();
            var user = MakeUser(1);
            var plan = MakePlan(1, "Premium", 99000, 30);
            context.Users.Add(user);
            context.PremiumPlans.Add(plan);
            var activeSubscription = new UserSubscription { UserID = user.UserID, PlanID = plan.PlanID, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Status = "Active" };
            var cancelledSubscription = new UserSubscription { UserID = user.UserID, PlanID = plan.PlanID, StartDate = DateTime.Now.AddDays(-60), EndDate = DateTime.Now.AddDays(-30), Status = "Cancelled" };
            context.UserSubscriptions.AddRange(activeSubscription, cancelledSubscription);
            await context.SaveChangesAsync();
            context.Payments.AddRange(
                new Payment { UserID = user.UserID, SubscriptionID = activeSubscription.SubscriptionID, Amount = 100000, PaymentMethod = "Card", PaymentDate = DateTime.Now },
                new Payment { UserID = user.UserID, SubscriptionID = cancelledSubscription.SubscriptionID, Amount = 50000, PaymentMethod = "Card", PaymentDate = DateTime.Now.AddDays(-40) });
            await context.SaveChangesAsync();
            var service = new PremiumService(context);

            
            var revenue = await service.GetRevenueAsync();

            
            Assert.Equal(150000, revenue.TotalRevenue);
            Assert.Equal(2, revenue.TotalSubscriptions);
            Assert.Equal(1, revenue.ActiveSubscriptions);
        }
    }
}
