using BeatIt.Models;
using Microsoft.EntityFrameworkCore;
namespace BeatIt.DataContext
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> User { get; set; }
        public DbSet<Game> Game { get; set; }
        public DbSet<Backlog> Backlog { get; set; }
        public DbSet<CompletedGames> CompletedGames { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
            .HasKey(u => u.Id);
            modelBuilder.Entity<User>()
            .Property(u => u.Id)
            .ValueGeneratedOnAdd();
            modelBuilder.Entity<Game>()
            .Property(g => g.Id)
            .ValueGeneratedOnAdd();
            modelBuilder.Entity<Backlog>()
            .Property(g => g.Id)
            .ValueGeneratedOnAdd();
            modelBuilder.Entity<CompletedGames>()
            .Property(g => g.Id)
            .ValueGeneratedOnAdd();

            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasIndex(g => g.IgdbGameId)
                    .IsUnique();
            });

            modelBuilder.Entity<CompletedGames>(entity =>
            {
                entity.HasOne(cg => cg.Game)
                    .WithMany(g => g.CompletedGames)
                    .HasForeignKey(cg => cg.GameId);

                entity.HasOne(cg => cg.User)
                    .WithMany(u => u.CompletedGames)
                    .HasForeignKey(cg => cg.UserId);
            });
            modelBuilder.Entity<Backlog>(entity =>
            {
                entity.HasOne(bg => bg.Game)
                    .WithMany(g => g.Backlogs)
                    .HasForeignKey(bg => bg.GameId);

                entity.HasOne(bg => bg.User)
                    .WithMany(u => u.Backlogs)
                    .HasForeignKey(bg => bg.UserId);
            });
        }
    }
}