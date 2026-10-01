namespace BinLord.Models;

/// <summary>
/// Shape used for JSON export/import of bin schedules. Dates are plain
/// "yyyy-MM-dd" strings so the file is portable and easy to hand-edit.
/// </summary>
public class BinScheduleExportDto
{
    public string Name { get; set; } = string.Empty;
    public string Colour { get; set; } = string.Empty;
    public string FirstCollectionDate { get; set; } = string.Empty;
    public int FrequencyWeeks { get; set; }
    public string? Notes { get; set; }
}
