namespace LMSApi.ModelLibrary.Enums
{
    public enum ActivityType
    {
        UserRegister,
        UserLogin,
        CourseCreated,
        CoursePublished,
        CourseEnrollment,
        QuizAttemptStarted,
        QuizAttemptSubmitted,
        AssignmentSubmitted,
        AssignmentGraded,
        CertificateIssued,
        PaymentSuccess,
        PaymentFailed,
        BatchAnnouncementCreated,
        DiscussionPostCreated,
        PayoutAccountRegistered,     // Step 1: account created
        PayoutAccountUpdated,        // Step 1: account updated
        StakeholderRegistered,       // Step 2 done
        PayoutProductRequested,      // Step 3 done
        BankDetailsConfigured,       // Step 4 done
        PayoutAccountActivated       // Webhook: account.activated
    }

    public enum ActionType
    {
        Insert,
        Update,
        Delete
    }
}