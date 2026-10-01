namespace BinLord.Models;

public class HistoryItemViewModel
{
    public int BinScheduleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Colour { get; set; } = string.Empty;
    public DateOnly CollectionDate { get; set; }
    public CollectionStatus? Status { get; set; }
}
