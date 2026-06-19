using Asp.Versioning;
using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.API.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMSApi.ModelLibrary.Enums;

namespace LMSApi.API.Controllers
{
    public class CreateAssignmentFormRequest : CreateAssignmentRequest
    {
        public IFormFile? AttachmentFile { get; set; }
    }

    public class UpdateAssignmentFormRequest : UpdateAssignmentRequest
    {
        public IFormFile? AttachmentFile { get; set; }
    }

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AssignmentsController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;
        private readonly AssignmentUploadHandler _assignmentUploadHandler;
        private readonly IOwnershipService _ownershipService;

        public AssignmentsController(
            IAssignmentService assignmentService,
            AssignmentUploadHandler assignmentUploadHandler,
            IOwnershipService ownershipService)
        {
            _assignmentService = assignmentService;
            _assignmentUploadHandler = assignmentUploadHandler;
            _ownershipService = ownershipService;
        }

        /// <summary>List all assignments in a section.</summary>
        [Authorize]
        [HttpGet("section/{sectionId:int}")]
        public async Task<ActionResult<IEnumerable<AssignmentResponse>>> GetBySection(int sectionId)
        {
            var userId = User.GetUserId();
            var isAdmin = User.IsAdmin();
            var result = await _assignmentService.GetAssignmentsBySectionAsync(sectionId, userId, isAdmin);
            return Ok(result);
        }

        /// <summary>Get a single assignment by Id.</summary>
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AssignmentResponse>> GetById(int id)
        {
            var userId = User.GetUserId();
            var isAdmin = User.IsAdmin();
            var result = await _assignmentService.GetAssignmentByIdAsync(id, userId, isAdmin);
            return Ok(result);
        }

        /// <summary>Create a new assignment (Instructor/Admin only).</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [Consumes("multipart/form-data")]
        [HttpPost]
        public async Task<ActionResult<AssignmentResponse>> Create([FromForm] CreateAssignmentFormRequest form)
        {
            await _ownershipService.EnforceSectionOwnershipAsync(form.CourseSectionId, User.GetUserId(), User.IsAdmin(), "You do not have permission to manage assignments in this section.");
            if (form.AttachmentType==AssignmentAttachmentType.File)
                _assignmentUploadHandler.ValidateAssignmentAttachment(form.AttachmentFile);

            var request = new CreateAssignmentRequest
            {
                CourseSectionId = form.CourseSectionId,
                Title = form.Title,
                Description = form.Description,
                Instructions = form.Instructions,
                IsCompulsory = form.IsCompulsory,
                TotalMarks = form.TotalMarks,
                PassingMarks = form.PassingMarks,
                AttachmentType = form.AttachmentType,
                AttachmentUrl = form.AttachmentUrl,
                DeadlineInDays = form.DeadlineInDays,
                MaxSubmissions = form.MaxSubmissions,
                IsLateSubmissionAllowed = form.IsLateSubmissionAllowed
            };
            Console.WriteLine("Assignment creation request received: " + form.AttachmentFile?.Name);
            await using var attachmentStream = form.AttachmentFile?.OpenReadStream();

            var result = await _assignmentService.CreateAssignmentAsync(
                request, 
                attachmentStream, 
                form.AttachmentFile?.FileName);
                
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>Update an existing assignment (Instructor/Admin only).</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<AssignmentResponse>> Update(int id, [FromForm] UpdateAssignmentFormRequest form)
        {
            await _ownershipService.EnforceAssignmentOwnershipAsync(id, User.GetUserId(), User.IsAdmin());

            if (form.AttachmentFile != null)
                _assignmentUploadHandler.ValidateAssignmentAttachment(form.AttachmentFile);

            var request = new UpdateAssignmentRequest
            {
                Title = form.Title,
                Description = form.Description,
                Instructions = form.Instructions,
                IsCompulsory = form.IsCompulsory,
                TotalMarks = form.TotalMarks,
                PassingMarks = form.PassingMarks,
                AttachmentType = form.AttachmentType,
                AttachmentUrl = form.AttachmentUrl,
                DeadlineInDays = form.DeadlineInDays,
                MaxSubmissions = form.MaxSubmissions,
                IsLateSubmissionAllowed = form.IsLateSubmissionAllowed
            };

            await using var attachmentStream = form.AttachmentFile?.OpenReadStream();

            var result = await _assignmentService.UpdateAssignmentAsync(
                id, 
                request, 
                attachmentStream, 
                form.AttachmentFile?.FileName);

            return Ok(result);
        }

        /// <summary>Delete an assignment (Instructor/Admin only).</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _ownershipService.EnforceAssignmentOwnershipAsync(id, User.GetUserId(), User.IsAdmin());
            await _assignmentService.DeleteAssignmentAsync(id);
            return NoContent();
        }

        /// <summary>Publish or unpublish an assignment (Instructor/Admin only).</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost("{id:int}/publish")]
        public async Task<ActionResult<AssignmentResponse>> Publish(int id, [FromBody] PublishAssignmentRequest request)
        {
            await _ownershipService.EnforceAssignmentOwnershipAsync(id, User.GetUserId(), User.IsAdmin());
            var result = await _assignmentService.PublishAssignmentAsync(id, request.Publish);
            return Ok(result);
        }

    }
}
