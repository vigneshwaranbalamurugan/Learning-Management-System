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
        private readonly ICourseSectionService _sectionService;
        private readonly ICourseService _courseService;
        private readonly AssignmentUploadHandler _assignmentUploadHandler;

        public AssignmentsController(
            IAssignmentService assignmentService,
            ICourseSectionService sectionService,
            ICourseService courseService,
            AssignmentUploadHandler assignmentUploadHandler)
        {
            _assignmentService = assignmentService;
            _sectionService = sectionService;
            _courseService = courseService;
            _assignmentUploadHandler = assignmentUploadHandler;
        }

        /// <summary>List all assignments in a section.</summary>
        [Authorize]
        [HttpGet("section/{sectionId:int}")]
        public async Task<ActionResult<IEnumerable<AssignmentResponse>>> GetBySection(int sectionId)
        {
            var result = await _assignmentService.GetAssignmentsBySectionAsync(sectionId);
            return Ok(result);
        }

        /// <summary>Get a single assignment by Id.</summary>
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AssignmentResponse>> GetById(int id)
        {
            var result = await _assignmentService.GetAssignmentByIdAsync(id);
            return Ok(result);
        }

        /// <summary>Create a new assignment (Instructor/Admin only).</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [Consumes("multipart/form-data")]
        [HttpPost]
        public async Task<ActionResult<AssignmentResponse>> Create([FromForm] CreateAssignmentFormRequest form)
        {
            await EnforceSectionOwnershipAsync(form.CourseSectionId);
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
            Console.WriteLine("Assignment creation request received: " + form.AttachmentFile.Name);
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
            await EnforceAssignmentOwnershipAsync(id);

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
            await EnforceAssignmentOwnershipAsync(id);
            await _assignmentService.DeleteAssignmentAsync(id);
            return NoContent();
        }

        /// <summary>Publish or unpublish an assignment (Instructor/Admin only).</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost("{id:int}/publish")]
        public async Task<ActionResult<AssignmentResponse>> Publish(int id, [FromBody] PublishAssignmentRequest request)
        {
            await EnforceAssignmentOwnershipAsync(id);
            var result = await _assignmentService.PublishAssignmentAsync(id, request.Publish);
            return Ok(result);
        }

        // ─── Private Ownership Helpers ───────────────────────────────────────

        private async Task EnforceSectionOwnershipAsync(int sectionId)
        {
            if (User.IsAdmin()) return;

            var callerId = User.GetUserId();
            var section = await _sectionService.GetSectionByIdAsync(sectionId);
            var course = await _courseService.GetCourseByIdAsync(section.CourseId);

            if (course.InstructorId != callerId)
                throw new UnauthorizedAccessException("You do not have permission to manage assignments in this section.");
        }

        private async Task EnforceAssignmentOwnershipAsync(int assignmentId)
        {
            if (User.IsAdmin()) return;

            var callerId = User.GetUserId();
            var assignment = await _assignmentService.GetAssignmentByIdAsync(assignmentId);
            var section = await _sectionService.GetSectionByIdAsync(assignment.CourseSectionId);
            var course = await _courseService.GetCourseByIdAsync(section.CourseId);

            if (course.InstructorId != callerId)
                throw new UnauthorizedAccessException("You do not have permission to modify this assignment.");
        }
    }
}
