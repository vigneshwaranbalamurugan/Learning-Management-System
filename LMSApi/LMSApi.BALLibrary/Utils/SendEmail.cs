using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace LMSApi.BALLibrary.Utils
{
	public static class SendEmail
	{
		public static async Task SendAsync(IConfiguration configuration, EmailMessage message)
		{
			var host = configuration["Smtp:Host"];
			var portStr = configuration["Smtp:Port"];
			var userName = configuration["Smtp:Username"];
			var password = configuration["Smtp:Password"];
			var fromDomain = configuration["Smtp:From"];

			if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(portStr))
			{
				return;
			}

			if (!int.TryParse(portStr, out var port))
			{
				port = 25;
			}

			var fromAddress = string.IsNullOrWhiteSpace(fromDomain)
				? userName ?? string.Empty
				: "Noreply@" + fromDomain;

			using var smtp = new SmtpClient(host, port)
			{
				EnableSsl = true
			};

			if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
			{
				smtp.Credentials = new NetworkCredential(userName, password);
			}

			using var mail = new MailMessage(fromAddress, message.RecipientEmail, message.Subject, message.Body);
			mail.IsBodyHtml = message.IsHtml;
			mail.BodyEncoding = System.Text.Encoding.UTF8;

			await smtp.SendMailAsync(mail);
		}
	}
}
