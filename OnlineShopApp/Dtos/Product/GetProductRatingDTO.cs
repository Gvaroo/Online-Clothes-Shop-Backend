using OnlineShopApp.Dtos.Auth;
using OnlineShopApp.Models.Auth;

namespace OnlineShopApp.Dtos.Product
{
	public class GetProductRatingDTO
	{

		public int RatingValue { get; set; }
		public DateTime TimeStamp { get; set; }
		public GetUserDTO User { get; set; }
	}
}
