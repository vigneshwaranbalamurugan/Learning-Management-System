using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using Microsoft.Extensions.Configuration;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Globalization;

namespace LMSApi.BALLibrary.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IConfiguration _config;

        public InvoiceService(IPaymentRepository paymentRepo, IConfiguration config)
        {
            _paymentRepo = paymentRepo;
            _config = config;
        }

        public async Task<byte[]> GenerateInvoiceAsync(int paymentId)
        {
            var payment = await _paymentRepo.GetByIdAsync(paymentId) 
                ?? throw new KeyNotFoundException($"Payment {paymentId} not found.");

            string appName = _config["App:Name"] ?? "CourseHub";

            using var document = new PdfDocument();
            document.Info.Title = $"Invoice_INV-{paymentId}";
            document.Info.Author = appName;
            
            var page = document.AddPage();
            using var gfx = XGraphics.FromPdfPage(page);

            // Fonts
            var titleFont = new XFont("Helvetica", 24, XFontStyle.Bold);
            var headerFont = new XFont("Helvetica", 14, XFontStyle.Bold);
            var normalFont = new XFont("Helvetica", 12, XFontStyle.Regular);
            var boldFont = new XFont("Helvetica", 12, XFontStyle.Bold);

            // Brand color
            var brandBrush = new XSolidBrush(XColor.FromArgb(28, 28, 123)); // #1C1C7B
            var secondaryBrush = new XSolidBrush(XColor.FromArgb(255, 140, 0)); // #FF8C00
            var blackBrush = XBrushes.Black;
            var grayBrush = XBrushes.DarkGray;

            // Draw Header
            var logoPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "images", "coursehub.png");
            double currentX = 50;
            if (System.IO.File.Exists(logoPath))
            {
                using var image = XImage.FromFile(logoPath);
                double width = image.PixelWidth * (40.0 / image.PixelHeight);
                gfx.DrawImage(image, currentX, 40, width, 40);
                currentX += width + 10;
            }
            
            // Draw App Name next to logo (or at start if no logo)
            gfx.DrawString(appName, titleFont, brandBrush, new XPoint(currentX, 65));
            
            gfx.DrawString("INVOICE", titleFont, secondaryBrush, new XPoint(page.Width - 150, 60));

            // Line separator
            gfx.DrawLine(new XPen(XColor.FromArgb(28, 28, 123), 2), 50, 80, page.Width - 50, 80);

            // Invoice details (Right)
            int currentY = 110;
            gfx.DrawString($"Invoice Number: INV-{paymentId}", boldFont, blackBrush, new XPoint(page.Width - 250, currentY));
            currentY += 20;
            gfx.DrawString($"Date: {(payment.PaidAt ?? payment.CreatedAt).ToString("dd MMM yyyy")}", normalFont, blackBrush, new XPoint(page.Width - 250, currentY));
            currentY += 20;
            gfx.DrawString($"Status: {payment.Status}", normalFont, blackBrush, new XPoint(page.Width - 250, currentY));
            currentY += 20;
            gfx.DrawString($"Payment ID: {payment.ProviderPaymentId ?? "N/A"}", normalFont, blackBrush, new XPoint(page.Width - 250, currentY));

            // Billed To (Left)
            currentY = 110;
            gfx.DrawString("Billed To:", headerFont, brandBrush, new XPoint(50, currentY));
            currentY += 25;
            gfx.DrawString($"{payment.User?.UserProfile?.FirstName} {payment.User?.UserProfile?.LastName}", boldFont, blackBrush, new XPoint(50, currentY));
            currentY += 20;
            gfx.DrawString(payment.User?.Email ?? "", normalFont, blackBrush, new XPoint(50, currentY));

            // Table Header
            currentY = 250;
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(243, 244, 246)), 50, currentY - 15, page.Width - 100, 30);
            gfx.DrawString("Description", boldFont, brandBrush, new XPoint(60, currentY + 5));
            gfx.DrawString("Amount", boldFont, brandBrush, new XPoint(page.Width - 120, currentY + 5));

            // Table Item
            currentY += 40;
            gfx.DrawString(payment.Course?.Title ?? "Course Enrollment", normalFont, blackBrush, new XPoint(60, currentY));
            
            var amountStr = $"{payment.Currency} {payment.Amount.ToString("N2", CultureInfo.InvariantCulture)}";
            gfx.DrawString(amountStr, normalFont, blackBrush, new XPoint(page.Width - 120, currentY));

            // Total Line
            currentY += 40;
            gfx.DrawLine(XPens.LightGray, 50, currentY, page.Width - 50, currentY);
            currentY += 20;
            gfx.DrawString("Total:", boldFont, brandBrush, new XPoint(page.Width - 200, currentY));
            gfx.DrawString(amountStr, boldFont, blackBrush, new XPoint(page.Width - 120, currentY));

            // Footer
            currentY = (int)page.Height - 100;
            gfx.DrawLine(new XPen(XColor.FromArgb(28, 28, 123), 2), 50, currentY, page.Width - 50, currentY);
            currentY += 20;
            gfx.DrawString("Thank you for your business!", normalFont, grayBrush, new XPoint(50, currentY));
            currentY += 20;
            gfx.DrawString($"If you have any questions, please contact support@{appName.ToLower()}.com", normalFont, grayBrush, new XPoint(50, currentY));

            using var ms = new System.IO.MemoryStream();
            document.Save(ms, false);
            return ms.ToArray();
        }
    }
}
