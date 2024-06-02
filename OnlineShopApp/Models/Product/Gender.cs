namespace OnlineShopApp.Models.Product
{
	public class Gender
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public List<Product>? Product { get; set; }
	}
}
