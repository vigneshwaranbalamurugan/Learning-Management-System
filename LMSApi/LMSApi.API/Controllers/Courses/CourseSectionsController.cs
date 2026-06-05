using Asp.Versioning;
using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMSApi.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CourseSectionsController : ControllerBase
    {
        private readonly ICourseSectionService _sectionService;
        private readonly ICourseService _courseService;

        public CourseSectionsController(
            ICourseSectionService sectionService,
            ICourseService courseService)
        {
            _sectionService = sectionService;
            _courseService = courseService;
        }

        // ─── Queries (all authenticated users) ──────────────────────────────

        [Authorize]
        [HttpGet("course/{courseId:int}")]
        public async Task<ActionResult<IEnumerable<SectionResponse>>> GetByCourse(int courseId)
        {
            var result = await _sectionService.GetSectionsByCourseAsync(courseId);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SectionResponse>> GetById(int id)
        {
            var result = await _sectionService.GetSectionByIdAsync(id);
            return Ok(result);
        }

        // ─── Mutations (Instructor = own courses only; Admin = all) ─────────

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        public async Task<ActionResult<SectionResponse>> Create([FromBody] CreateSectionRequest request)
        {
            await EnforceCourseOwnershipAsync(request.CourseId);

            var result = await _sectionService.CreateSectionAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<SectionResponse>> Update(int id, [FromBody] UpdateSectionRequest request)
        {
            await EnforceSectionOwnershipAsync(id);

            var result = await _sectionService.UpdateSectionAsync(id, request);
            return Ok(result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await EnforceSectionOwnershipAsync(id);

            await _sectionService.DeleteSectionAsync(id);
            return NoContent();
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("reorder")]
        public async Task<IActionResult> Reorder([FromBody] ReorderSectionsRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            foreach (var item in request.SectionOrders)
            {
                await EnforceSectionOwnershipAsync(item.SectionId);
            }

            await _sectionService.ReorderSectionsAsync(request);
            return NoContent();
        }


        /// <summary>
        /// Ensures the calling Instructor owns the course.
        /// Admins bypass this check.
        /// </summary>
        private async Task EnforceCourseOwnershipAsync(int courseId)
        {
            if (User.IsAdmin()) return;

            var callerId = User.GetUserId();
            var course = await _courseService.GetCourseByIdAsync(courseId);

            if (course.InstructorId != callerId)
                throw new UnauthorizedAccessException("You do not have permission to modify sections in this course.");
        }

        /// <summary>
        /// Resolves a section's parent course and verifies the calling Instructor is the creator.
        /// Admins bypass this check.
        /// </summary>
        private async Task EnforceSectionOwnershipAsync(int sectionId)
        {
            if (User.IsAdmin()) return;

            var callerId = User.GetUserId();
            var section = await _sectionService.GetSectionByIdAsync(sectionId);
            var course = await _courseService.GetCourseByIdAsync(section.CourseId);

            if (course.InstructorId != callerId)
                throw new UnauthorizedAccessException("You do not have permission to modify this section.");
        }
    }
}
