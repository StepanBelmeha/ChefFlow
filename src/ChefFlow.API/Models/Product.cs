namespace ChefFlow.API.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
         public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    }
}