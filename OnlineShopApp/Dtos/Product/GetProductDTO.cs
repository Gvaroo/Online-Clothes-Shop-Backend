using OnlineShopApp.Models.Product;

namespace OnlineShopApp.Dtos.Product
{
	public class GetProductDTO
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
		public int Quantity { get; set; }
		public int AverageRating { get; set; }
		public int ReviewsCount { get; set; }
		public GetCategoryDTO Category { get; set; }
		public GetSubCategoriesDTO SubCategories { get; set; }
		public List<GetProductImageDTO> Images { get; set; } = new List<GetProductImageDTO>();
		public GetBrandDTO Brand { get; set; }
		public List<GetProductSizeDTO> Sizes { get; set; } = new List<GetProductSizeDTO>();

	}
}
