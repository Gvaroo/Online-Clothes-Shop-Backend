namespace OnlineShopApp.Services.Interfaces
{
	public interface IAuditService
	{
		Task LogRestock(int productId, int QuantityRestocked, int AdminId);
	}
}
