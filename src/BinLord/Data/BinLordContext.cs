using BinLord.Models;
using Microsoft.EntityFrameworkCore;

namespace BinLord.Data;

public class BinLordContext : DbContext
{
    public BinLordContext(DbContextOptions<BinLordContext> options) : base(options)
    {
    }

    public DbSet<BinSchedule> BinSchedules => Set<BinSchedule>();

    public DbSet<BinCollectionRecord> BinCollectionRecords => Set<BinCollectionRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BinCollectionRecord>()
            .HasIndex(r => new { r.BinScheduleId, r.CollectionDate })
            .IsUnique();
    }
}
