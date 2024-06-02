namespace OnlineShopApp.Services.Interfaces
{
	public interface IEmailSender
	{
		Task SendEmailAsyncBackground(string email, string subject, string message);
		Task SendEmailAsync(string email, string subject, string message);
	}
}
