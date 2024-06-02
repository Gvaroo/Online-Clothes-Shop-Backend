using System.ComponentModel.DataAnnotations;

namespace OnlineShopApp.Models.Cart
{
	public class OrderItems
	{
		[Key]
		public int Id { get; set; }
		public int ProductId { get; set; }
		public string Name { get; set; }
		public string Image { get; set; }
		public decimal OriginalPrice { get; set; }
		public decimal TotalPrice { get; set; }
		public int Quantity { get; set; }
		public string ProductSize { get; set; }

		//Relation
		public Order Order { get; set; }
	}
}
