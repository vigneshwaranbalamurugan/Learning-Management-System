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
    public class ProgressController : ControllerBase
    {
        private readonly IStudentProgressService _progressService;

        public ProgressController(IStudentProgressService progressService)
        {
            _progressService = progressService;
        }

        /// <summary>Returns the overall course completion progress for the authenticated student.</summary>
        [Authorize]
        [HttpGet("course/{courseId:int}")]
        public async Task<ActionResult<CourseProgressResponse>> GetCourseProgress(int courseId)
        {
            var userId = User.GetUserId();
            var result = await _progressService.GetCourseProgressAsync(userId, courseId);
            return Ok(result);
        }

        /// <summary>
        /// Returns the progress record for a single lesson including the last watched second.
        /// Use this before starting video playback to get the resume position.
        /// Returns 204 No Content when the student has never started this lesson.
        /// </summary>
        [Authorize]
        [HttpGet("lessons/{lessonId:int}")]
        public async Task<ActionResult<LessonProgressResponse>> GetLessonProgress(int lessonId)
        {
            var userId = User.GetUserId();
            var result = await _progressService.GetLessonProgressAsync(userId, lessonId);
            if (result == null) return NoContent();
            return Ok(result);
        }

        /// <summary>Returns the overall course completion progress of all students enrolled in a course (Instructor/Admin only).</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpGet("course/{courseId:int}/students")]
        public async Task<ActionResult<IEnumerable<StudentProgressSummaryDto>>> GetStudentsProgress(int courseId)
        {
            var instructorId = User.GetUserId();
            var isAdmin = User.IsInRole("Admin");
            var result = await _progressService.GetStudentsProgressForCourseAsync(instructorId, courseId, isAdmin);
            return Ok(result);
        }

        /// <summary>Returns a student's detailed course completion progress (Instructor/Admin only).</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpGet("course/{courseId:int}/students/{studentId:int}/detail")]
        public async Task<ActionResult<CourseProgressResponse>> GetStudentDetailedProgress(int courseId, int studentId)
        {
            var instructorId = User.GetUserId();
            var isAdmin = User.IsInRole("Admin");
            var result = await _progressService.GetStudentDetailedProgressForInstructorAsync(instructorId, studentId, courseId, isAdmin);
            return Ok(result);
        }
    }
}
