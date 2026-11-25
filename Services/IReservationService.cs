using MultiMediaRoomReservationSystem.Models;

namespace MultiMediaRoomReservationSystem.Services;

public interface IReservationService
{
    Task<List<Reservation>> GetAllAsync();
    Task<Reservation?> GetByIdAsync(int id);
    Task<string?> CreateAsync(Reservation reservation);
    Task<string?> UpdateAsync(Reservation reservation);
    Task<bool> DeleteAsync(int id);
    Task<string?> ApproveAsync(int id);
    Task<bool> RejectAsync(int id);
}
