using StackExchange.Redis;

namespace OnlineShopApp.Utilities
{
	public static class RedisHelper
	{
		public static async Task<bool> IsRedisAvailableAsync(string configuration)
		{
			try
			{
				var connection = await ConnectionMultiplexer.ConnectAsync(configuration);
				return connection.IsConnected;
			}
			catch
			{
				return false;
			}
		}
	}
}
