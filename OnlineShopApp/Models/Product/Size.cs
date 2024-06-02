namespace OnlineShopApp.Models.Product
{
	public class Size
	{
		public int SizeId { get; set; }
		public string SizeName { get; set; }

		public List<Product> Products { get; set; } = new List<Product>();
	}
}
