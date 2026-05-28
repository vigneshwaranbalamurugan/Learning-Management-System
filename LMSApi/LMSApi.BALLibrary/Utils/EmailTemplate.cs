namespace LMSApi.BALLibrary.Utils
{
    public static class EmailTemplate
    {
        public static string GetVerificationTemplate(string email, string link)
        {
            return $@"<html>
            <body>
              <h2>Verify your email</h2>
              <p>Hi {System.Net.WebUtility.HtmlEncode(email)},</p>
              <p>Thank you for registering. Please click the button below to verify your email address.</p>
              <p><a href=""{System.Net.WebUtility.HtmlEncode(link)}"" style=""display:inline-block;padding:10px 20px;background:#0d6efd;color:#fff;text-decoration:none;border-radius:4px;"">Verify Email</a></p>
              <p>If the button doesn't work, copy and paste the following link into your browser:</p>
              <p>{System.Net.WebUtility.HtmlEncode(link)}</p>
              <hr />
              <small>Do not share this link. It expires in 24 hours.</small>
            </body>
          </html>";
        }
    }
}
