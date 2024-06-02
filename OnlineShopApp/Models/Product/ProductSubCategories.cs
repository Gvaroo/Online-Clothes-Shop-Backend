namespace OnlineShopApp.Models.Product
{
	public class ProductSubCategories
	{
		public int Id { get; set; }
		public string Name { get; set; }

		//relation
		public Category Category { get; set; }
		public List<Product> Product { get; set; } = new List<Product>();
	}
}
