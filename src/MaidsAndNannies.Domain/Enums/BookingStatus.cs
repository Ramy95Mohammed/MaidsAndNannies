namespace MaidsPlatform.API.Domain.Enums;

public enum BookingStatus
{
    Pending = 0,
    WorkerConfirmed = 1,
    WaitingPayment = 2,
    Paid = 3,
    Active = 4,
    Completed = 5,
    Cancelled = 6,
    ReplacementRequested = 7,
    PaymentSubmitted = 8
}
