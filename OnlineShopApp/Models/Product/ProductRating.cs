using OnlineShopApp.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace OnlineShopApp.Models.Product
{
	public class ProductRating
	{
		public int Id { get; set; }

		[Range(1, 5, ErrorMessage = "Rating value must be between 1 and 5.")]
		public int RatingValue { get; set; }
		public DateTime TimeStamp { get; set; }

		//relation
		public Product Product { get; set; }
		public User User { get; set; }
		public ProductReviews ProductReviews { get; set; }
	}
}
