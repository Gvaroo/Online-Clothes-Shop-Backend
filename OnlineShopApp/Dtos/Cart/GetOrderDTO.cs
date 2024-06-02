namespace OnlineShopApp.Dtos.Cart
{
	public class GetOrderDTO
	{
		public int OrderId { get; set; }
		public string? OrderDate { get; set; }      //datetime
		public decimal? TotalPrice { get; set; }
		public List<ProductDTO> Products { get; set; }
		
	}
}
