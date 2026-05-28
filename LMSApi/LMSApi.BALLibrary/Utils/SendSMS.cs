using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Utils
{
	public static class SendSMS
	{
		public static Task SendAsync(SMSMessage message)
		{
			return Task.CompletedTask;
		}
	}
}
