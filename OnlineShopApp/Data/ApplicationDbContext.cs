using Microsoft.EntityFrameworkCore;
using OnlineShopApp.Models.Audit;
using OnlineShopApp.Models.Auth;
using OnlineShopApp.Models.Cart;
using OnlineShopApp.Models.Product;

namespace OnlineShopApp.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions options) : base(options)
		{
		}
		public DbSet<User> Users { get; set; }
		public DbSet<UserRoles> Roles { get; set; }
		public DbSet<Category> Category { get; set; }
		public DbSet<Product> Product { get; set; }
		public DbSet<ProductImages> ProductImages { get; set; }
		public DbSet<ProductRating> ProductRating { get; set; }
		public DbSet<ProductReviews> ProductReviews { get; set; }
		public DbSet<ProductColor> ProductColors { get; set; }
		public DbSet<ProductBrand> ProductBrand { get; set; }
		public DbSet<Gender> Gender { get; set; }
		public DbSet<Size> Sizes { get; set; }
		public DbSet<ProductSubCategories> ProductSubCategories { get; set; }
		public DbSet<RestockAudit> RestockAudit { get; set; }
		public DbSet<Order> Order { get; set; }
		public DbSet<OrderItems> OrderItems { get; set; }
		public DbSet<ShippingInfo> ShippingInfo { get; set; }
		public DbSet<ExternalLogins> ExternalLogins { get; set; }
		public DbSet<Cart> Cart { get; set; }
		public DbSet<CartItem> CartItems { get; set; }
		

	}
}
