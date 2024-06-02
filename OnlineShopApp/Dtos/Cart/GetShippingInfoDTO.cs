namespace OnlineShopApp.Dtos.Cart
{
	public class GetShippingInfoDTO
	{
		public string ShippingAddress { get; set; }
		public string ShippingCountry { get; set; }
		public string ShippingCity { get; set; }
		public string ZipCode { get; set; }
		public int PhoneNumber { get; set; }
	}
}
