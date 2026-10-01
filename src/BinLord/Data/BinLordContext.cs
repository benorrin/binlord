using BinLord.Models;
using Microsoft.EntityFrameworkCore;

namespace BinLord.Data;

public class BinLordContext : DbContext
{
    public BinLordContext(DbContextOptions<BinLordContext> options) : base(options)
    {
    }

    public DbSet<BinSchedule> BinSchedules => Set<BinSchedule>();
}
