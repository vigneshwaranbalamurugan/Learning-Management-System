namespace LMSApi.ModelLibrary.Enums
{
    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded,
        Transferred   // Payment completed and instructor payout initiated
    }
}