using System.ComponentModel.DataAnnotations;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Http;

namespace LMSApi.API.Handlers
{
    public class CreateResourceFormRequest:CreateResourceRequest
    {
        public IFormFile? File { get; set; }
    }

    public class UpdateResourceFormRequest:UpdateResourceRequest
    {
        public IFormFile? File { get; set; }
    }
}
