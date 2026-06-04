namespace LMSApi.ModelLibrary.Enums
{
    /// <summary>
    /// Lifecycle status of a student enrollment.
    /// </summary>
    public enum EnrollmentStatus
    {
        /// <summary>Enrollment is active and the student can access the course.</summary>
        Active = 1,

        /// <summary>Student has completed the course.</summary>
        Completed = 2,

        /// <summary>Access period has expired (applies to SelfPaced with a deadline).</summary>
        Expired = 3,

        /// <summary>Enrollment was manually cancelled.</summary>
        Cancelled = 4
    }
}
