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
        private readonly IOwnershipService _ownershipService;

        public CourseSectionsController(
            ICourseSectionService sectionService,
            IOwnershipService ownershipService)
        {
            _sectionService = sectionService;
            _ownershipService = ownershipService;
        }

        // ─── Queries (all authenticated users) ──────────────────────────────

        [Authorize]
        [HttpGet("course/{courseId:int}")]
        public async Task<ActionResult<IEnumerable<SectionResponse>>> GetByCourse(int courseId)
        {
            var userId = User.GetUserId();
            var isAdmin = User.IsAdmin();
            var result = await _sectionService.GetSectionsByCourseAsync(courseId, userId, isAdmin);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SectionResponse>> GetById(int id)
        {
            var userId = User.GetUserId();
            var isAdmin = User.IsAdmin();
            var result = await _sectionService.GetSectionByIdAsync(id, userId, isAdmin);
            return Ok(result);
        }

        // ─── Mutations (Instructor = own courses only; Admin = all) ─────────

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        public async Task<ActionResult<SectionResponse>> Create([FromBody] CreateSectionRequest request)
        {
            await _ownershipService.EnforceCourseOwnershipAsync(request.CourseId, User.GetUserId(), User.IsAdmin(), "You do not have permission to modify sections in this course.");

            var result = await _sectionService.CreateSectionAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<SectionResponse>> Update(int id, [FromBody] UpdateSectionRequest request)
        {
            await _ownershipService.EnforceSectionOwnershipAsync(id, User.GetUserId(), User.IsAdmin());

            var result = await _sectionService.UpdateSectionAsync(id, request);
            return Ok(result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _ownershipService.EnforceSectionOwnershipAsync(id, User.GetUserId(), User.IsAdmin());

            await _sectionService.DeleteSectionAsync(id);
            return NoContent();
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPatch("{id:int}/publish")]
        public async Task<ActionResult<SectionResponse>> Publish(int id, [FromBody] PublishSectionRequest request)
        {
            await _ownershipService.EnforceSectionOwnershipAsync(id, User.GetUserId(), User.IsAdmin());
            var result = await _sectionService.PublishSectionAsync(id, request);
            return Ok(result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("reorder")]
        public async Task<IActionResult> Reorder([FromBody] ReorderSectionsRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            foreach (var item in request.SectionOrders)
            {
                await _ownershipService.EnforceSectionOwnershipAsync(item.SectionId, User.GetUserId(), User.IsAdmin());
            }

            await _sectionService.ReorderSectionsAsync(request);
            return NoContent();
        }


    }
}
