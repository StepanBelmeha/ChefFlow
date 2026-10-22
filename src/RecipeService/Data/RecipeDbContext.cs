using Microsoft.EntityFrameworkCore;
using RecipeService.Models;

namespace RecipeService.Data
{
    public class RecipeDbContext(DbContextOptions<RecipeDbContext> options) : DbContext(options)
    {
        public DbSet<Recipe> Recipes => Set<Recipe>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Ingredient> Ingredients => Set<Ingredient>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Складений первинний ключ для Ingredient
            modelBuilder.Entity<Ingredient>()
                .HasKey(i => new { i.RecipeId, i.ProductId });

            // Зв'язки
            modelBuilder.Entity<Ingredient>()
                .HasOne(i => i.Recipe)
                .WithMany(r => r.Ingredients)
                .HasForeignKey(i => i.RecipeId);

            modelBuilder.Entity<Ingredient>()
                .HasOne(i => i.Product)
                .WithMany(p => p.Ingredients)
                .HasForeignKey(i => i.ProductId);

            // Індекс для пошуку за Title
            modelBuilder.Entity<Recipe>()
                .HasIndex(r => r.Title);

            // Максимальні довжини
            modelBuilder.Entity<Recipe>()
                .Property(r => r.Title)
                .HasMaxLength(200);

            modelBuilder.Entity<Product>()
                .Property(p => p.Name)
                .HasMaxLength(100);

            // Значення за замовчуванням
            modelBuilder.Entity<Recipe>()
                .Property(r => r.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        }
    }
}