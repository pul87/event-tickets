using System.Data;
using EventTickets.Payments.Domain;
using EventTickets.Shared;
using EventTickets.Shared.Outbox;
using Microsoft.EntityFrameworkCore;

namespace EventTickets.Payments.Infrastructure;

public sealed class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }

    public DbSet<PaymentIntent> PaymentIntents => Set<PaymentIntent>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.HasDefaultSchema(SchemaNames.Payments);

        var pi = model.Entity<PaymentIntent>();
        pi.ToTable("payment_intents");
        pi.HasKey(x => x.Id);
        pi.HasIndex(x => x.ReservationId).IsUnique();
        pi.Property(x => x.Amount).HasColumnType("numeric(12,2)");
        pi.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        pi.Property(x => x.PayUrl).HasMaxLength(2048);

        var ob = model.Entity<OutboxMessage>();
        ob.ToTable("outbox_messages");
        ob.HasIndex(x => x.Id);
        ob.Property(x => x.Type).IsRequired().HasMaxLength(512);
        ob.Property(x => x.Content).IsRequired();
        ob.Property(x => x.OccurredOnUtc).IsRequired();
        ob.Property(x => x.ProcessedOnUtc);
        ob.Property(x => x.Error);
    }
}
