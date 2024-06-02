using System.ComponentModel.DataAnnotations;

namespace OnlineShopApp.Dtos.Product
{
	public class AddProductDTO
	{
		[Required]
		public string Name { get; set; }
		[Required]
		public int CategoryId { get; set; }
		[Required]
		public int SubCategoryId { get; set; }
		public int? BrandId { get; set; }
		[Required]
		public List<int> ColorIds { get; set; } = new List<int>();
		[Required]
		public int? GenderId { get; set; }
		[Required]
		public List<int> SizeIds { get; set; }
		[Required]
		public string Description { get; set; }
		[Required]
		[DataType(DataType.Currency)]
		public decimal Price { get; set; }
		[Required]
		public int Quantity { get; set; }		
		public string? newBrand { get; set; }
		[Required]
		public List<UploadImageDTO> Images { get; set; } = new List<UploadImageDTO>();


	}
}
