namespace BinLord.Models;

/// <summary>
/// Records whether a specific expected occurrence of a bin schedule was
/// actually collected or missed. Only marked occurrences have a row here;
/// an occurrence with no row is treated as not yet marked.
/// </summary>
public class BinCollectionRecord
{
    public int Id { get; set; }

    public int BinScheduleId { get; set; }

    public BinSchedule BinSchedule { get; set; } = null!;

    public DateOnly CollectionDate { get; set; }

    public CollectionStatus Status { get; set; }
}
