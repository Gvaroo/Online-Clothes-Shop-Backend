using OnlineShopApp.Services.Interfaces;
using System.Net.Mail;
using System.Net;
using Hangfire;

namespace OnlineShopApp.Services.EmailSender
{
	public class EmailSender : IEmailSender
	{
		private readonly IConfiguration _configuration;

		public EmailSender(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		//Using Hangfire to make email sending process to a background task
		//This allows the user to receive an immediate response while the email is sent asynchronously.
		public Task SendEmailAsyncBackground(string email, string subject, string message)
		{
			BackgroundJob.Enqueue(() => SendEmailAsync(email, subject, message));
			return Task.CompletedTask;
		}

		[AutomaticRetry(Attempts = 3)]
		public Task SendEmailAsync(string email, string subject, string message)
		{
			var options = _configuration.GetSection("EmailCredential").Get<EmailSenderOptions>();

			var client = new SmtpClient("smtp.office365.com", 587)
			{
				EnableSsl = true,
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential(options.Email, options.Password)
			};
			var mailMessage = new MailMessage(from: options.Email,
			to: email,
			subject,
			message);
			mailMessage.IsBodyHtml = true;
			return client.SendMailAsync(mailMessage);
			
		}
	}
}
