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
    public class LessonsController : ControllerBase
    {
        private readonly ILessonService _lessonService;

        public LessonsController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        /// <summary>Get all lessons for a given section, ordered by SortOrder.</summary>
        [Authorize]
        [HttpGet("section/{sectionId:int}")]
        public async Task<ActionResult<IEnumerable<LessonResponse>>> GetBySection(int sectionId)
        {
            var result = await _lessonService.GetLessonsBySectionAsync(sectionId);
            return Ok(result);
        }

        /// <summary>Get a lesson by id.</summary>
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<LessonResponse>> GetById(int id)
        {
            var result = await _lessonService.GetLessonByIdAsync(id);
            return Ok(result);
        }

        /// <summary>Create a new lesson. Instructor and Admin only.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        public async Task<ActionResult<LessonResponse>> Create([FromBody] CreateLessonRequest request)
        {
            var result = await _lessonService.CreateLessonAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>Update a lesson. Instructor and Admin only.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<LessonResponse>> Update(int id, [FromBody] UpdateLessonRequest request)
        {
            var result = await _lessonService.UpdateLessonAsync(id, request);
            return Ok(result);
        }

        /// <summary>Delete a lesson. Instructor and Admin only.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _lessonService.DeleteLessonAsync(id);
            return NoContent();
        }

        /// <summary>Reorder lessons within a section. Instructor and Admin only.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("reorder")]
        public async Task<IActionResult> Reorder([FromBody] ReorderLessonsRequest request)
        {
            await _lessonService.ReorderLessonsAsync(request);
            return NoContent();
        }
    }
}
