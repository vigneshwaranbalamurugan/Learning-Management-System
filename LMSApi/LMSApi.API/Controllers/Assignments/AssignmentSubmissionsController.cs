using Asp.Versioning;
using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using LMSApi.API.Handlers;
using LMSApi.ModelLibrary.Enums;

namespace LMSApi.API.Controllers
{
    public class AssignmentSubmissionFormRequest : AssignmentSubmissionRequest
    {
        public IFormFile? AttachmentFile { get; set; }
    }

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AssignmentSubmissionsController : ControllerBase
    {
        private readonly IAssignmentSubmissionService _assignmentSubmissionService;
        private readonly AssignmentUploadHandler _assignmentUploadHandler;

        public AssignmentSubmissionsController(
            IAssignmentSubmissionService assignmentSubmissionService,
            AssignmentUploadHandler assignmentUploadHandler)
        {
            _assignmentSubmissionService = assignmentSubmissionService;
            _assignmentUploadHandler = assignmentUploadHandler;
        }

        /// <summary>Submit an assignment (enrolled students).</summary>
        [Authorize]
        [Consumes("multipart/form-data")]
        [HttpPost]
        [EnableRateLimiting("AssignmentSubmit")]
        public async Task<ActionResult<AssignmentSubmissionResponse>> Submit([FromForm] AssignmentSubmissionFormRequest form)
        {
            if (form.AttachmentType == AssignmentSubmissonAttachmentType.File)
            {
                if (form.AttachmentFile == null)
                {
                    throw new InvalidOperationException("Assignment attachment is required.");
                }
                _assignmentUploadHandler.ValidateAssignmentAttachment(form.AttachmentFile);
            }

            var studentId = User.GetUserId();
            
            var request = new AssignmentSubmissionRequest
            {
                AssignmentId = form.AssignmentId,
                SubmissionText = form.SubmissionText,
                AttachmentType = form.AttachmentType,
                SubmittedAssignmentUrl = form.SubmittedAssignmentUrl
            };
            
            await using var attachmentStream = form.AttachmentFile?.OpenReadStream();
            
            var result = await _assignmentSubmissionService.SubmitAssignmentAsync(
                studentId, 
                request, 
                attachmentStream, 
                form.AttachmentFile?.FileName);
                
            return Ok(result);
        }

        /// <summary>Grade a submission (Instructor/Admin only).</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id:int}/grade")]
        public async Task<ActionResult<AssignmentSubmissionResponse>> Grade(int id, [FromBody] GradeSubmissionRequest request)
        {
            var result = await _assignmentSubmissionService.GradeAssignmentAsync(id, request);
            return Ok(result);
        }

        /// <summary>Get the authenticated student's own submissions for an assignment.</summary>
        [Authorize]
        [HttpGet("assignment/{assignmentId:int}/my-submissions")]
        public async Task<ActionResult<IEnumerable<AssignmentSubmissionResponse>>> GetMySubmissions(int assignmentId)
        {
            var studentId = User.GetUserId();
            var result = await _assignmentSubmissionService.GetStudentSubmissionsAsync(assignmentId, studentId);
            return Ok(result);
        }

        /// <summary>Get pending (Submitted / UnderReview) submissions for an assignment — instructor view.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpGet("assignment/{assignmentId:int}/pending")]
        public async Task<ActionResult<IEnumerable<AssignmentSubmissionResponse>>> GetPendingReviews(int assignmentId)
        {
            var result = await _assignmentSubmissionService.GetPendingReviewsAsync(assignmentId);
            return Ok(result);
        }

        /// <summary>Get the authenticated student's status summary for an assignment.</summary>
        [Authorize]
        [HttpGet("assignment/{assignmentId:int}/status")]
        public async Task<ActionResult<AssignmentStatusResponse>> GetStatus(int assignmentId)
        {
            var studentId = User.GetUserId();
            var result = await _assignmentSubmissionService.GetStudentAssignmentStatusAsync(assignmentId, studentId);
            return Ok(result);
        }
    }
}
