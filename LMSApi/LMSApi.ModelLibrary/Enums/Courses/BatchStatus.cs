namespace LMSApi.ModelLibrary.Enums
{
    /// <summary>
    /// Lifecycle status of a cohort batch.
    /// </summary>
    public enum BatchStatus
    {
        /// <summary>Batch is scheduled but has not started yet.</summary>
        Upcoming = 1,

        /// <summary>Batch is currently running.</summary>
        Active = 2,

        /// <summary>Batch has finished.</summary>
        Completed = 3,

        /// <summary>Batch was cancelled before or after it started.</summary>
        Cancelled = 4
    }
}
