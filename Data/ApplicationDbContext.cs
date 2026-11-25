using Microsoft.EntityFrameworkCore;
using MultiMediaRoomReservationSystem.Models;

namespace MultiMediaRoomReservationSystem.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.ReservationDate).HasColumnType("date");
            entity.Property(r => r.StartTime).HasColumnType("time");
            entity.Property(r => r.EndTime).HasColumnType("time");
            entity.Property(r => r.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasIndex(r => new { r.ReservationDate, r.StartTime, r.EndTime });
        });
    }
}
