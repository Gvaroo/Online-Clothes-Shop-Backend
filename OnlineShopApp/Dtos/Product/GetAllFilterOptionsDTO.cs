namespace OnlineShopApp.Dtos.Product
{
	public class GetAllFilterOptionsDTO
	{		
		public List<GetCategoryDTO> Categories { get; set; } = new List<GetCategoryDTO>();
		public List<GetSubCategoriesDTO> SubCategories { get; set; } = new List<GetSubCategoriesDTO>();
		public List<GetBrandDTO> Brands { get; set; } = new List<GetBrandDTO>();
		public List<GetColorDTO> Colors { get; set; } = new List<GetColorDTO>();
		public List<GetProductSizeDTO> Sizes { get; set; } = new List<GetProductSizeDTO>();
		public List<GetGenderDTO> Genders { get; set; } = new List<GetGenderDTO>();
		public List<GetProductSortDTO> SortOptions { get; set; } = new List<GetProductSortDTO>();


	}
}
