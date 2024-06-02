using OnlineShopApp.Models.Cart;
using OnlineShopApp.Models.Product;
using System.ComponentModel.DataAnnotations;

namespace OnlineShopApp.Models.Auth
{
	public class User
	{
		public int Id { get; set; }
		[Required]
		public string FullName { get; set; }
		[Required]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }
		public string? PasswordHash { get; set; }

		public string? PasswordSalt { get; set; }
		public bool IsVerified { get; set; } = false;

		//Relation
		public List<Product.Product> Products { get; set; } = new List<Product.Product>();
		public UserRoles Role { get; set; }
		public List<ProductRating> ProductRating { get; set; } = new List<ProductRating>();
		public List<ProductReviews> ProductReviews { get; set; } = new List<ProductReviews>();
		public List<Order>? Order { get; set; } = new List<Order>();
		public ShippingInfo? ShippingInfo { get; set; }
		public List<SecurityVerificationCodes>? securityVerificationCodes { get; set; } = new List<SecurityVerificationCodes>();
		public List<ExternalLogins>? ExternalLogins { get; set; } = new List<ExternalLogins>();
		public Cart.Cart? Cart { get; set; }
	}
}
