namespace OnlineShopApp.Models.Audit
{
	public class RestockAudit
	{
		public int Id { get; set; }
		public int ProductId { get; set; }
		public int QuantityRestocked { get; set; }
		public int AdminId { get; set; }
		public DateTime Timestamp { get; set; }
	}
}
