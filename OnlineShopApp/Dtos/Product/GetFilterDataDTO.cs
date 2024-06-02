namespace OnlineShopApp.Dtos.Product
{
	public class GetFilterDataDTO
	{
		public int? SortId { get; set; }
		public int? CategoryId { get; set; }
		public int? SubCategoryId { get; set; }
		public int? BrandId { get; set; }
		public int? ColorId { get; set; }
		public int? SizeId { get; set; }
		public int? GenderId { get; set; }
		public int? MinimumPrice { get; set; }
		public int? MaximumPrice { get; set; }
		public string? ProductName { get; set; }
		public bool IsAllFiltersCleared { get; set; }

	}
}
