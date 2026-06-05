using Asp.Versioning;
using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMSApi.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AssignmentsController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ICourseSectionService _sectionService;
        private readonly ICourseService _courseService;

        public AssignmentsController(
            IAssignmentService assignmentService,
            ICourseSectionService sectionService,
            ICourseService courseService)
        {
            _assignmentService = assignmentService;
            _sectionService = sectionService;
            _courseService = courseService;
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
        [HttpPost]
        public async Task<ActionResult<AssignmentResponse>> Create([FromBody] CreateAssignmentRequest request)
        {
            await EnforceSectionOwnershipAsync(request.CourseSectionId);
            var result = await _assignmentService.CreateAssignmentAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>Update an existing assignment (Instructor/Admin only).</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<AssignmentResponse>> Update(int id, [FromBody] UpdateAssignmentRequest request)
        {
            await EnforceAssignmentOwnershipAsync(id);
            var result = await _assignmentService.UpdateAssignmentAsync(id, request);
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
