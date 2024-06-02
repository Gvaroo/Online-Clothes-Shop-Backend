using System.ComponentModel.DataAnnotations;

namespace OnlineShopApp.Dtos.Product
{
	public class AddProductReviewAndRatingDTO
	{
		[Required]
		public int ProductId { get; set; }
		[Required]
		[Range(1, 5, ErrorMessage = "Rating value must be between 1 and 5.")]
		public int RatingValue { get; set; }
		[Required]
		public string Review { get; set; }
	}
}
