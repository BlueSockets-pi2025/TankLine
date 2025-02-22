using Microsoft.EntityFrameworkCore;
using GameApi.Models;

namespace GameApi.Data
{
    public class GameDbContext : DbContext
    {
	public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }
	
	public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<FriendList> FriendLists { get; set; }
	public DbSet<Leaderboard> Leaderboards { get; set; }
	public DbSet<GeneratedMap> GeneratedMaps { get; set; }	

	protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAccount>().ToTable("user_accounts");
            modelBuilder.Entity<Achievement>().ToTable("achievement");
            modelBuilder.Entity<Leaderboard>().ToTable("leaderboard");
            modelBuilder.Entity<GeneratedMap>().ToTable("generated_maps");
		
	    // Definition of the composite key because [Key] cannot be defined twice in the C# class:
	    modelBuilder.Entity<FriendList>()
		.HasKey(f => new { f.User1, f.User2 });    

	    modelBuilder.Entity<FriendList>()
                .HasOne(f => f.UserAccount1)
                .WithMany()
                .HasForeignKey(f => f.User1)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<FriendList>()
                .HasOne(f => f.UserAccount2)
                .WithMany()
                .HasForeignKey(f => f.User2)
                .OnDelete(DeleteBehavior.Cascade); 
	}
    }
}
