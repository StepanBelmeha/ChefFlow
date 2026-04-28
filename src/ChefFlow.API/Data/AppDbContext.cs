
namespace ChefFlow.API.Data
{
    using ChefFlow.API.Models;
    using Microsoft.EntityFrameworkCore;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){  }

        public DbSet<User> Users { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Favorite> Favorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Favorite>()
                .HasKey(f => new { f.UserId, f.RecipeId });

            modelBuilder.Entity<Ingredient>()
                .HasKey(i => new { i.RecipeId, i.ProductId });
        }
    }
}