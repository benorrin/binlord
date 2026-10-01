using System.ComponentModel.DataAnnotations;

namespace BinLord.Models;

public class BinSchedule
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Bin name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Colour { get; set; } = "#4caf50";

    [Required]
    [Display(Name = "A known collection date")]
    [DataType(DataType.Date)]
    public DateOnly FirstCollectionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required]
    [Range(1, 52)]
    [Display(Name = "Repeats every (weeks)")]
    public int FrequencyWeeks { get; set; } = 2;

    [StringLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Returns the next collection date on or after <paramref name="today"/>,
    /// derived from the anchor date and the repeat interval.
    /// </summary>
    public DateOnly GetNextCollectionDate(DateOnly today)
    {
        var intervalDays = FrequencyWeeks * 7;
        if (intervalDays <= 0)
        {
            return FirstCollectionDate;
        }

        if (FirstCollectionDate >= today)
        {
            return FirstCollectionDate;
        }

        var daysSinceAnchor = today.DayNumber - FirstCollectionDate.DayNumber;
        var intervalsPassed = daysSinceAnchor / intervalDays;
        var candidate = FirstCollectionDate.AddDays(intervalsPassed * intervalDays);

        if (candidate < today)
        {
            candidate = candidate.AddDays(intervalDays);
        }

        return candidate;
    }

    /// <summary>
    /// Returns past expected occurrences on or before <paramref name="today"/>,
    /// most recent first, up to <paramref name="maxCount"/> of them.
    /// </summary>
    public IEnumerable<DateOnly> GetPastOccurrences(DateOnly today, int maxCount)
    {
        var intervalDays = FrequencyWeeks * 7;
        if (intervalDays <= 0 || FirstCollectionDate > today)
        {
            yield break;
        }

        var daysSinceAnchor = today.DayNumber - FirstCollectionDate.DayNumber;
        var lastIndex = daysSinceAnchor / intervalDays;

        for (var i = 0; i <= lastIndex && i < maxCount; i++)
        {
            yield return FirstCollectionDate.AddDays((lastIndex - i) * intervalDays);
        }
    }

    /// <summary>
    /// Returns upcoming occurrences on or after <paramref name="today"/>,
    /// soonest first, up to <paramref name="maxCount"/> of them.
    /// </summary>
    public IEnumerable<DateOnly> GetUpcomingOccurrences(DateOnly today, int maxCount)
    {
        var intervalDays = FrequencyWeeks * 7;
        if (intervalDays <= 0)
        {
            yield break;
        }

        var date = GetNextCollectionDate(today);
        for (var i = 0; i < maxCount; i++)
        {
            yield return date;
            date = date.AddDays(intervalDays);
        }
    }
}
