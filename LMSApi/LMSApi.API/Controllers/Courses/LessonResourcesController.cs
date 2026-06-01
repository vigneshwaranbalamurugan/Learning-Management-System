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
    public class LessonResourcesController : ControllerBase
    {
        private readonly ILessonResourceService _resourceService;

        public LessonResourcesController(ILessonResourceService resourceService)
        {
            _resourceService = resourceService;
        }

        /// <summary>Get all resources for a given lesson.</summary>
        [Authorize]
        [HttpGet("lesson/{lessonId:int}")]
        public async Task<ActionResult<IEnumerable<ResourceResponse>>> GetByLesson(int lessonId)
        {
            var result = await _resourceService.GetResourcesByLessonAsync(lessonId);
            return Ok(result);
        }

        /// <summary>Get a resource by id.</summary>
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ResourceResponse>> GetById(int id)
        {
            var result = await _resourceService.GetResourceByIdAsync(id);
            return Ok(result);
        }

        /// <summary>Add a resource to a lesson. Instructor and Admin only.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        public async Task<ActionResult<ResourceResponse>> Add([FromBody] CreateResourceRequest request)
        {
            var result = await _resourceService.AddResourceAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>Update a resource. Instructor and Admin only.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ResourceResponse>> Update(int id, [FromBody] UpdateResourceRequest request)
        {
            var result = await _resourceService.UpdateResourceAsync(id, request);
            return Ok(result);
        }

        /// <summary>Delete a resource. Instructor and Admin only.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _resourceService.DeleteResourceAsync(id);
            return NoContent();
        }
    }
}
