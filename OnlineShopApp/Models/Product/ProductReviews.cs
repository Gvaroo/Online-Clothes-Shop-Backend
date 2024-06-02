using AutoMapper.Configuration.Conventions;
using OnlineShopApp.Models.Auth;

namespace OnlineShopApp.Models.Product
{
	public class ProductReviews
	{
		public int Id { get; set; }
		public string ReviewText { get; set; }
		public DateTime DateStamp { get; set; }

		//Relation

		public Product Product { get; set; }
		public User User { get; set; }
		public int ProductRatingId { get; set; }
		public ProductRating ProductRating { get; set; }

	}
}
