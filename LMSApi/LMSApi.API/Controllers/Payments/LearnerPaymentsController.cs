using Asp.Versioning;
using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace LMSApi.API.Controllers.Payments
{
    [Authorize(Roles = "Learner")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/payments/my")]
    public class LearnerPaymentsController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepo;
        private readonly IInvoiceService _invoiceService;
        private readonly IUploadService _uploadService;

        public LearnerPaymentsController(IPaymentRepository paymentRepo, IInvoiceService invoiceService, IUploadService uploadService)
        {
            _paymentRepo = paymentRepo;
            _invoiceService = invoiceService;
            _uploadService = uploadService;
        }

        [HttpGet]
        public async Task<ActionResult<LearnerPaymentPagedResponse>> GetMyPayments(
            [FromQuery] string? search, [FromQuery] ModelLibrary.Enums.PaymentStatus? status, 
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.GetUserId();
            var (payments, totalCount) = await _paymentRepo.GetLearnerPaymentsPagedAsync(userId, search, status, page, pageSize);

            var items = payments.Select(p => new LearnerPaymentResponse
            {
                Id = p.Id,
                CourseTitle = p.Course.Title,
                CourseThumbnailUrl = string.IsNullOrWhiteSpace(p.Course.ThumbnailUrl) || p.Course.ThumbnailUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) 
                    ? p.Course.ThumbnailUrl 
                    : _uploadService.GeneratePublicSasUrl(p.Course.ThumbnailUrl),
                Amount = p.Amount,
                Currency = p.Currency,
                Status = p.Status.ToString(),
                PaidAt = p.PaidAt,
                CreatedAt = p.CreatedAt,
                ProviderPaymentId = p.ProviderPaymentId
            }).ToList();

            var totalPages = totalCount == 0 ? 1 : (int)System.Math.Ceiling((double)totalCount / pageSize);

            var response = new LearnerPaymentPagedResponse
            {
                Items = items,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page
            };

            return Ok(response);
        }

        [HttpGet("{id}/invoice")]
        public async Task<IActionResult> DownloadInvoice(int id)
        {
            var userId = User.GetUserId();
            var payment = await _paymentRepo.GetByIdAsync(id);

            if (payment == null || payment.UserId != userId)
            {
                return NotFound(new { message = "Payment not found or you do not have permission to view it." });
            }

            if (payment.Status != ModelLibrary.Enums.PaymentStatus.Completed && payment.Status != ModelLibrary.Enums.PaymentStatus.Transferred)
            {
                return BadRequest(new { message = "Invoice is only available for completed payments." });
            }

            var pdfBytes = await _invoiceService.GenerateInvoiceAsync(id);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var courseTitleSafe = new string(payment.Course.Title.Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-').ToArray()).Replace(" ", "_");
            return File(pdfBytes, "application/pdf", $"Invoice_{courseTitleSafe}_INV-{id}_{timestamp}.pdf");
        }
    }
}
