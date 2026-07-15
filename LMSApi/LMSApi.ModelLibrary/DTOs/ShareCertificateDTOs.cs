using System;

namespace LMSApi.ModelLibrary.DTOs
{
    public class ShareCertificateRequest
    {
        public int Minutes { get; set; }
    }

    public class ShareCertificateResponse
    {
        public string Token { get; set; } = string.Empty;
        public string ShareUrl { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
