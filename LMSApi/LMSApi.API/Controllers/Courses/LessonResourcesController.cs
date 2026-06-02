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
    public class LessonResourcesController : ControllerBase
    {
        private readonly ILessonResourceService _resourceService;
        private readonly ILessonService _lessonService;
        private readonly ICourseSectionRepository _sectionRepository;
        private readonly ICourseService _courseService;

        public LessonResourcesController(
            ILessonResourceService resourceService,
            ILessonService lessonService,
            ICourseSectionRepository sectionRepository,
            ICourseService courseService)
        {
            _resourceService = resourceService;
            _lessonService = lessonService;
            _sectionRepository = sectionRepository;
            _courseService = courseService;
        }

        // ─── Queries (all authenticated users) ──────────────────────────────

        [Authorize]
        [HttpGet("lesson/{lessonId:int}")]
        public async Task<ActionResult<IEnumerable<ResourceResponse>>> GetByLesson(int lessonId)
        {
            var result = await _resourceService.GetResourcesByLessonAsync(lessonId);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ResourceResponse>> GetById(int id)
        {
            var result = await _resourceService.GetResourceByIdAsync(id);
            return Ok(result);
        }

        // ─── Mutations (Instructor = own courses only; Admin = all) ─────────

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        public async Task<ActionResult<ResourceResponse>> Add([FromBody] CreateResourceRequest request)
        {
            await EnforceLessonOwnershipAsync(request.LessonId);

            var result = await _resourceService.AddResourceAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ResourceResponse>> Update(int id, [FromBody] UpdateResourceRequest request)
        {
            await EnforceResourceOwnershipAsync(id);

            var result = await _resourceService.UpdateResourceAsync(id, request);
            return Ok(result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await EnforceResourceOwnershipAsync(id);

            await _resourceService.DeleteResourceAsync(id);
            return NoContent();
        }


        /// <summary>
        /// Resolves lesson → section → course and verifies the calling Instructor is the creator.
        /// Admins bypass this check.
        /// </summary>
        private async Task EnforceLessonOwnershipAsync(int lessonId)
        {
            if (User.IsAdmin()) return;

            var callerId = User.GetUserId();
            var lesson = await _lessonService.GetLessonByIdAsync(lessonId);
            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId);
            var course = await _courseService.GetCourseByIdAsync(section.CourseId);

            if (course.InstructorId != callerId)
                throw new UnauthorizedAccessException("You do not have permission to add resources to this lesson.");
        }

        /// <summary>
        /// Resolves resource → lesson → section → course and verifies the calling Instructor is the creator.
        /// Admins bypass this check.
        /// </summary>
        private async Task EnforceResourceOwnershipAsync(int resourceId)
        {
            if (User.IsAdmin()) return;

            var callerId = User.GetUserId();
            var resource = await _resourceService.GetResourceByIdAsync(resourceId);
            var lesson = await _lessonService.GetLessonByIdAsync(resource.LessonId);
            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId);
            var course = await _courseService.GetCourseByIdAsync(section.CourseId);

            if (course.InstructorId != callerId)
                throw new UnauthorizedAccessException("You do not have permission to modify this resource.");
        }
    }
}
