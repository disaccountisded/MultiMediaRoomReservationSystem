using Microsoft.EntityFrameworkCore;
using MultiMediaRoomReservationSystem.Data;
using MultiMediaRoomReservationSystem.Models;

namespace MultiMediaRoomReservationSystem.Services;

public class ReservationService : IReservationService
{
    private readonly ApplicationDbContext _context;

    // Sample operating hours. Change these to match the school's actual policy.
    private static readonly TimeSpan OpeningTime = new(8, 0, 0);
    private static readonly TimeSpan ClosingTime = new(17, 0, 0);

    public ReservationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Reservation>> GetAllAsync()
    {
        return await _context.Reservations
            .AsNoTracking()
            .OrderByDescending(r => r.ReservationDate)
            .ThenBy(r => r.StartTime)
            .ToListAsync();
    }

    public async Task<Reservation?> GetByIdAsync(int id)
    {
        return await _context.Reservations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    private static string? ValidateReservation(Reservation reservation)
    {
        if (reservation.ReservationDate.Date < DateTime.Today)
            return "You cannot reserve a date in the past.";

        if (reservation.StartTime < OpeningTime || reservation.EndTime > ClosingTime)
            return "Reservations must be within the operating hours of 8:00 AM to 5:00 PM.";

        if (reservation.StartTime >= reservation.EndTime)
            return "The end time must be later than the start time.";

        return null;
    }

    private async Task<bool> HasConflictAsync(
        Reservation reservation, int? excludedId = null)
    {
        // Half-open interval comparison allows one booking to start exactly
        // when the previous booking ends (for example, 9:00-10:00 and 10:00-11:00).
        return await _context.Reservations.AnyAsync(existing =>
            existing.ReservationDate == reservation.ReservationDate.Date &&
            existing.Id != (excludedId ?? 0) &&
            existing.Status != ReservationStatus.Rejected &&
            reservation.StartTime < existing.EndTime &&
            reservation.EndTime > existing.StartTime);
    }

    public async Task<string?> CreateAsync(Reservation reservation)
    {
        var validationError = ValidateReservation(reservation);
        if (validationError is not null)
            return validationError;

        if (await HasConflictAsync(reservation))
            return "That time overlaps another pending or approved reservation.";

        reservation.ReservationDate = reservation.ReservationDate.Date;
        reservation.Status = ReservationStatus.Pending;
        reservation.CreatedAt = DateTime.Now;

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        return null;
    }

    public async Task<string?> UpdateAsync(Reservation reservation)
    {
        var existing = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == reservation.Id);

        if (existing is null)
            return "Reservation was not found.";

        if (existing.Status != ReservationStatus.Pending)
            return "Only pending reservations can be edited.";

        var validationError = ValidateReservation(reservation);
        if (validationError is not null)
            return validationError;

        if (await HasConflictAsync(reservation, reservation.Id))
            return "That time overlaps another pending or approved reservation.";

        existing.ReservedBy = reservation.ReservedBy;
        existing.Email = reservation.Email;
        existing.Department = reservation.Department;
        existing.Purpose = reservation.Purpose;
        existing.ReservationDate = reservation.ReservationDate.Date;
        existing.StartTime = reservation.StartTime;
        existing.EndTime = reservation.EndTime;
        existing.NumberOfAttendees = reservation.NumberOfAttendees;

        await _context.SaveChangesAsync();
        return null;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation is null)
            return false;

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string?> ApproveAsync(int id)
    {
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation is null)
            return "Reservation was not found.";

        if (reservation.Status != ReservationStatus.Pending)
            return "Only pending reservations can be approved.";

        // Re-check immediately before approval.
        var conflict = await _context.Reservations.AnyAsync(other =>
            other.Id != reservation.Id &&
            other.ReservationDate == reservation.ReservationDate &&
            other.Status == ReservationStatus.Approved &&
            reservation.StartTime < other.EndTime &&
            reservation.EndTime > other.StartTime);

        if (conflict)
            return "This request conflicts with an already approved reservation.";

        reservation.Status = ReservationStatus.Approved;
        await _context.SaveChangesAsync();
        return null;
    }

    public async Task<bool> RejectAsync(int id)
    {
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation is null || reservation.Status != ReservationStatus.Pending)
            return false;

        reservation.Status = ReservationStatus.Rejected;
        await _context.SaveChangesAsync();
        return true;
    }
}
