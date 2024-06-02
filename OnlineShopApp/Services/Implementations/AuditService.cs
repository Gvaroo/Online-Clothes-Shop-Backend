using OnlineShopApp.Data;
using OnlineShopApp.Models.Audit;
using OnlineShopApp.Services.Interfaces;

namespace OnlineShopApp.Services.Implementations
{
	public class AuditService : IAuditService
	{
		private readonly ApplicationDbContext _db;

		public AuditService(ApplicationDbContext db)
		{
			_db = db;
		}


		// Saves audit in database when admin restocks product
		public async Task LogRestock(int productId, int QuantityRestocked, int AdminId)
		{
			var auditRecord = new RestockAudit()
			{
				ProductId = productId,
				AdminId = AdminId,
				QuantityRestocked = QuantityRestocked,
				Timestamp = DateTime.UtcNow
			};
			await _db.RestockAudit.AddAsync(auditRecord);
			await _db.SaveChangesAsync();
		}


	}
}
