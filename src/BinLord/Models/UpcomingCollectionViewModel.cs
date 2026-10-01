namespace BinLord.Models;

public class UpcomingCollectionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Colour { get; set; } = "#4caf50";
    public DateOnly NextCollectionDate { get; set; }
    public int DaysUntil { get; set; }
    public string? Notes { get; set; }

    public bool IsToday => DaysUntil == 0;
    public bool IsTomorrow => DaysUntil == 1;
    public bool IsSoon => DaysUntil <= 1;
}
