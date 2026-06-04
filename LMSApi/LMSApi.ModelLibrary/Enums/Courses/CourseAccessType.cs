namespace LMSApi.ModelLibrary.Enums
{
    /// <summary>
    /// Determines whether a course uses a self-paced or cohort-based (batch) learning model.
    /// </summary>
    public enum CourseAccessType
    {
        /// <summary>Enroll anytime, learn at your own pace. No batch required.</summary>
        SelfPaced = 1,

        /// <summary>Fixed batch with start/end dates, seat limits, and assignment deadlines.</summary>
        CohortBased = 2
    }
}
