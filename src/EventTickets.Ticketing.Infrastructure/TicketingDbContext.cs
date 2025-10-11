using EventTickets.Ticketing.Domain;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Ticketing.Infrastructure;

public sealed class TicketingDbContext : DbContext
{
    public TicketingDbContext(DbContextOptions<TicketingDbContext> options) : base(options) { }

    public DbSet<PerformanceInventory> PerformanceInventories => Set<PerformanceInventory>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.HasDefaultSchema("ticketing");

        // Inventory
        var pi = model.Entity<PerformanceInventory>();
        pi.ToTable("performance_inventory");
        pi.HasKey(x => x.Id);
        pi.Property(x => x.Capacity).IsRequired();
        pi.Property(x => x.Reserved).IsRequired();
        pi.Property(x => x.Sold).IsRequired();
        pi.Property(x => x.Version).IsRowVersion(); // Postgres: mappa su xmin

        // Reservation
        var r = model.Entity<Reservation>();
        r.ToTable("reservations");
        r.HasKey(x => x.Id);
        r.Property(x => x.PerformanceId).IsRequired();
        r.Property(x => x.Quantity).IsRequired();
        r.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        r.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        r.HasIndex(x => x.PerformanceId);
    }
}
