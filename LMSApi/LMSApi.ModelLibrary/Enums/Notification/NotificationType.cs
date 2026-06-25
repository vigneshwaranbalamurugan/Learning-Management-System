namespace LMSApi.ModelLibrary.Enums
{
    public enum NotificationType
    {
        CourseEnrollment,
        AssignmentCreated,
        AssignmentDeadline,
        AssignmentGraded,
        QuizCreated,
        QuizResult,
        CertificateIssued,
        PaymentSuccess,
        PaymentFailed,
        PaymentDispute,   // payment.dispute.* events
        PaymentDowntime,  // payment.downtime.* events (admin only)
        Settlement,       // settlement.processed (admin only)
        ProductRoute,     // product.route.* events (instructor)
        BatchAnnouncement,
        CoursePublished,
        General
    }
}
