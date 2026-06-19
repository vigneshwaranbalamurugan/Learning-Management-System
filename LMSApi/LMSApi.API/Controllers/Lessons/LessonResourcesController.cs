using Asp.Versioning;
using LMSApi.API.Extensions;
using LMSApi.API.Handlers;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMSApi.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class LessonResourcesController : ControllerBase
    {
        private readonly ILessonResourceService _resourceService;
        private readonly ILessonService _lessonService;
        private readonly LessonUploadHandler _lessonUploadHandler;
        private readonly IOwnershipService _ownershipService;

        public LessonResourcesController(
            ILessonResourceService resourceService,
            ILessonService lessonService,
            LessonUploadHandler lessonUploadHandler,
            IOwnershipService ownershipService)
        {
            _resourceService = resourceService;
            _lessonService = lessonService;
            _lessonUploadHandler = lessonUploadHandler;
            _ownershipService = ownershipService;
        }

        [Authorize]
        [HttpGet("lesson/{lessonId:int}")]
        public async Task<ActionResult<IEnumerable<ResourceResponse>>> GetByLesson(int lessonId)
        {
            var userId = User.GetUserId();
            var isAdmin = User.IsAdmin();
            var result = await _resourceService.GetResourcesByLessonAsync(lessonId, userId, isAdmin);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ResourceResponse>> GetById(int id)
        {
            var userId = User.GetUserId();
            var isAdmin = User.IsAdmin();
            var result = await _resourceService.GetResourceByIdAsync(id, userId, isAdmin);
            return Ok(result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ResourceResponse>> Add([FromForm] CreateResourceFormRequest form)
        {
            await _ownershipService.EnforceLessonOwnershipAsync(form.LessonId, User.GetUserId(), User.IsAdmin(), "You do not have permission to add resources to this lesson.");

            if (form.ResourceType == ResourceType.Pdf)
            {
                if (form.File == null)
                    throw new InvalidOperationException("PDF file is required for PDF type resources.");
                _lessonUploadHandler.ValidateLessonPdf(form.File);
            }
            else if (form.ResourceType == ResourceType.ExternalLink)
            {
                if (string.IsNullOrWhiteSpace(form.ResourceUrl))
                    throw new InvalidOperationException("Resource URL is required for ExternalLink type resources.");
            }

            var request = new CreateResourceRequest
            {
                LessonId = form.LessonId,
                ResourceType = form.ResourceType,
                ResourceTitle = form.ResourceTitle,
                ResourceUrl = form.ResourceUrl,
                Description = form.Description,
                Status = form.Status
            };

            await using var fileStream = form.File?.OpenReadStream();

            var result = await _resourceService.AddResourceAsync(request, fileStream, form.File?.FileName);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ResourceResponse>> Update(int id, [FromForm] UpdateResourceFormRequest form)
        {
            await _ownershipService.EnforceResourceOwnershipAsync(id, User.GetUserId(), User.IsAdmin());

            var existingResource = await _resourceService.GetResourceByIdAsync(id);
            var finalType = form.ResourceType ?? existingResource.ResourceType;

            if (form.File != null)
            {
                if (finalType == ResourceType.Pdf)
                {
                    _lessonUploadHandler.ValidateLessonPdf(form.File);
                }
                else
                {
                    throw new InvalidOperationException($"Files cannot be uploaded to resources of type {finalType}.");
                }
            }

            if (form.ResourceType.HasValue)
            {
                if (form.ResourceType.Value == ResourceType.ExternalLink
                    && string.IsNullOrWhiteSpace(form.ResourceUrl)
                    && string.IsNullOrWhiteSpace(existingResource.ResourceUrl))
                {
                    throw new InvalidOperationException("Resource URL is required for ExternalLink type resources.");
                }
            }

            var request = new UpdateResourceRequest
            {
                ResourceType = form.ResourceType,
                ResourceTitle = form.ResourceTitle,
                ResourceUrl = form.ResourceUrl,
                Description = form.Description,
                Status = form.Status
            };

            await using var fileStream = form.File?.OpenReadStream();

            var result = await _resourceService.UpdateResourceAsync(id, request, fileStream, form.File?.FileName);
            return Ok(result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _ownershipService.EnforceResourceOwnershipAsync(id, User.GetUserId(), User.IsAdmin());

            await _resourceService.DeleteResourceAsync(id);
            return NoContent();
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPatch("{id:int}/publish")]
        public async Task<ActionResult<ResourceResponse>> Publish(int id, [FromBody] PublishResourceRequest request)
        {
            await _ownershipService.EnforceResourceOwnershipAsync(id, User.GetUserId(), User.IsAdmin());
            var result = await _resourceService.PublishResourceAsync(id, request);
            return Ok(result);
        }

    }
}
