namespace BloodDonationSystem.Enums
{
    public enum BloodRequestStatus
    {
        Open,
        Fulfilled,   // donation confirmed — ready for pickup
        Completed,   // blood received by patient
        Closed       // cancelled / expired
    }
}
