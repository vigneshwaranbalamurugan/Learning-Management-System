using System;

namespace LMSApi.ModelLibrary.DTOs
{
    public class UpcomingDeadlineDto
    {
        public int UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserName { get; set; }
        public string CourseName { get; set; }
        public string ItemType { get; set; }
        public string ItemTitle { get; set; }
        public DateTime DeadlineDate { get; set; }
    }
}
