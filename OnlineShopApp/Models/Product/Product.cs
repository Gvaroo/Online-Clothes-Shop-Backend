using OnlineShopApp.Models.Auth;
using OnlineShopApp.Models.Cart;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShopApp.Models.Product
{
	public class Product
	{
		[Key]
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
		public int Quantity { get; set; }

		[NotMapped]
		public int createdBy { get; set; }
		//Relations
		public User User { get; set; }
		public Category Category { get; set; }
		public ProductSubCategories SubCategories { get; set; }		
		public Gender? Gender { get; set; }
		public List<ProductColor> ProductColor { get; set; } = new List<ProductColor>();	

		
		public ProductBrand? ProductBrand { get; set; }

		public List<ProductImages> Images { get; set; } = new List<ProductImages>();
		public List<ProductRating> ProductRating { get; set; } = new List<ProductRating>();
		public List<ProductReviews> ProductReviews { get; set; } = new List<ProductReviews>();
		public List<CartItem> CartItems { get; set; } = new List<CartItem>();
		public List<Size> Sizes { get; set; } = new List<Size>();
		
	}
}
