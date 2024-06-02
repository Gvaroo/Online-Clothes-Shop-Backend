using OnlineShopApp.Dtos.Auth;

namespace OnlineShopApp.Dtos.Product
{
	public class GetProductReviewsDTO
	{

		public string ReviewText { get; set; }
		public DateTime DateStamp { get; set; }
		public GetUserDTO User { get; set; }
		public GetRatingDTO ProductRating { get; set; }

	}
}
