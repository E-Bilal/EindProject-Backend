using Microsoft.EntityFrameworkCore;
using BackendAPI.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using BackendAPI.Model.DTO;

namespace BackendAPI.Configurations
{
    public class AppDbContext : IdentityDbContext // Identyitydbcontext also includes the dbcontext settings
    {
        public DbSet<Friend> Friends { get; set; }
        public DbSet<Tweet> Tweets { get; set; }
        public DbSet<Chat> Chats { get; set; }

        public DbSet<TweetLike> TweetLikes { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Friend>()
                .HasKey(bc => new { bc.ApplicationUserId, bc.ApplicationFriendId });

            modelBuilder.Entity<Friend>()
                .HasOne(bc => bc.ApplicationUser)
                .WithMany(b => b.User)
                .HasForeignKey(bc => bc.ApplicationUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Friend>()
                .HasOne(bc => bc.ApplicationFriend)
                .WithMany(c => c.Friend)
                .HasForeignKey(bc => bc.ApplicationFriendId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Friend>()
                .Property(b => b.Status)
                .HasDefaultValue("NotFriends");


            modelBuilder.Entity<TweetLike>()
                .HasOne(bc => bc.Tweet)
                .WithMany(c => c.TweetLike)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ApplicationUser>()
                .Property(b => b.RoleRequest)
                .HasDefaultValue("NoRequest");



        }

        //protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        //{
        //    configurationBuilder.Properties<DateTime>()
        //        .HavePrecision(3);
        //}
    }
}
