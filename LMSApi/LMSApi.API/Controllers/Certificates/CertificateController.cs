using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using LMSApi.API.Extensions;

namespace LMSApi.API.Controllers
{
    [ApiController]
    [Route("api/v1/certificates")]
    public class CertificateController : ControllerBase
    {
        private readonly ICertificateService _certificateService;

        public CertificateController(ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }

        [HttpGet("my")]
        [Authorize]
        [EnableRateLimiting("CertificateDownload")]
        public async Task<ActionResult<PagedCertificateResponse>> GetMyCertificates([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.GetUserId();
            var result = await _certificateService.GetMyCertificatesPagedAsync(userId, page, pageSize);
            return Ok(result);
        }

        [HttpGet("verify/{certificateId:guid}")]
        [AllowAnonymous]
        [EnableRateLimiting("CertificateDownload")]
        public async Task<IActionResult> VerifyCertificate(Guid certificateId)
        {
            var result = await _certificateService.VerifyCertificateAsync(certificateId);
            if (!result.IsValid)
            {
                return NotFound(new { message = "Certificate not found or invalid." });
            }

            return Ok(result.Certificate);
        }

        [HttpPost("templates")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        [EnableRateLimiting("FileUpload")]
        public async Task<IActionResult> CreateTemplate([FromForm] CreateCertificateTemplateRequest request, IFormFile backgroundImage)
        {
            if (backgroundImage == null || backgroundImage.Length == 0)
            {
                return BadRequest(new { message = "Background image is required." });
            }

            using var stream = backgroundImage.OpenReadStream();
            var result = await _certificateService.CreateTemplateAsync(request, stream, backgroundImage.FileName);

            return CreatedAtAction(nameof(GetTemplates), new { }, result);
        }

        [HttpGet("templates")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("AdminApis")]
        public async Task<IActionResult> GetTemplates()
        {
            var templates = await _certificateService.GetTemplatesAsync();
            return Ok(templates);
        }

        [HttpPatch("templates/{templateId:int}")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("AdminApis")]
        public async Task<IActionResult> UpdateTemplate(int templateId, [FromBody] UpdateCertificateTemplateRequest request)
        {
            var result = await _certificateService.UpdateTemplateAsync(templateId, request);
            return Ok(result);
        }
    }
}
