using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using OnlineShopApp.Models.Product;

namespace OnlineShopApp.Dtos.Product
{
	public class GetProductsDTO
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public decimal Price { get; set; }
		public int Quantity { get; set; }		
		public GetCategoryDTO Category { get; set; }
		public GetSubCategoriesDTO SubCategories { get; set; }
		public GetProductImageDTO? Image { get; set; }		
		public GetProductSizeDTO? DefaultSize { get; set; }
	}
}
