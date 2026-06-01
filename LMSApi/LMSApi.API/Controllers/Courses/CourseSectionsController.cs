using Asp.Versioning;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMSApi.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CourseSectionsController : ControllerBase
    {
        private readonly ICourseSectionService _sectionService;

        public CourseSectionsController(ICourseSectionService sectionService)
        {
            _sectionService = sectionService;
        }

        /// <summary>Get all sections for a given course.</summary>
        [Authorize]
        [HttpGet("course/{courseId:int}")]
        public async Task<ActionResult<IEnumerable<SectionResponse>>> GetByCourse(int courseId)
        {
            var result = await _sectionService.GetSectionsByCourseAsync(courseId);
            return Ok(result);
        }

        /// <summary>Get a section by id.</summary>
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SectionResponse>> GetById(int id)
        {
            var result = await _sectionService.GetSectionByIdAsync(id);
            return Ok(result);
        }

        /// <summary>Create a new section. Instructor and Admin only.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        public async Task<ActionResult<SectionResponse>> Create([FromBody] CreateSectionRequest request)
        {
            var result = await _sectionService.CreateSectionAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>Update a section. Instructor and Admin only.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<SectionResponse>> Update(int id, [FromBody] UpdateSectionRequest request)
        {
            var result = await _sectionService.UpdateSectionAsync(id, request);
            return Ok(result);
        }

        /// <summary>Delete a section. Instructor and Admin only.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _sectionService.DeleteSectionAsync(id);
            return NoContent();
        }
    }
}
