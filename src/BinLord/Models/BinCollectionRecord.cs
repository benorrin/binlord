namespace BinLord.Models;

/// <summary>
/// Tracks the state of a specific expected occurrence of a bin schedule:
/// whether it was actually collected or missed (for history), and whether
/// the bin has been put out and brought back in (for same-day reminders).
/// Only occurrences with some action taken have a row here.
/// </summary>
public class BinCollectionRecord
{
    public int Id { get; set; }

    public int BinScheduleId { get; set; }

    public BinSchedule BinSchedule { get; set; } = null!;

    public DateOnly CollectionDate { get; set; }

    public CollectionStatus? Status { get; set; }

    public DateTime? TakenOutAt { get; set; }

    public DateTime? BroughtInAt { get; set; }
}
