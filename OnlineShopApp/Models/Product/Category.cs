namespace OnlineShopApp.Models.Product
{
	public class Category
	{
		public int Id { get; set; }
		public string Name { get; set; }

		//Relation
		public List<Product> Products { get; set; } = new List<Product>();
		public List<ProductSubCategories> SubCategories { get; set; } = new List<ProductSubCategories>();
	}
}
