namespace OnlineShopApp.Dtos.Product
{
	public class GetCategoryAndSubCategoriesDTO
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public List<GetSubCategoriesDTO> SubCategories { get; set; }
	}
}
