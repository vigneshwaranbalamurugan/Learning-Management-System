using System.ComponentModel.DataAnnotations;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Http;

namespace LMSApi.API.Handlers
{
    public class CreateLessonFormRequest:CreateLessonRequest
    {
        /// <summary>Uploaded file (video or PDF) depending on the LessonType.</summary>    
        public IFormFile? File { get; set; }
    }

    public class UpdateLessonFormRequest:UpdateLessonRequest
    {        
        /// <summary>Uploaded file (video or PDF) depending on the LessonType.</summary>
        public IFormFile? File { get; set; }
    }
}
