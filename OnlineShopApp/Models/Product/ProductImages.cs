using OnlineShopApp.Dtos.Product;

namespace OnlineShopApp.Models.Product
{
	public class ProductImages
	{
		public int Id { get; set; }
		public string ImageUrl { get; set; }

		//Relation

		public Product Product { get; set; }
		public int ProductId { get; set; }

	
	}
}
