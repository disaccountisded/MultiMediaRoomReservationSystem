using Microsoft.AspNetCore.Mvc;
using MultiMediaRoomReservationSystem.Models;
using MultiMediaRoomReservationSystem.Services;

namespace MultiMediaRoomReservationSystem.Controllers;

public class ReservationsController : Controller
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    public async Task<IActionResult> Index()
    {
        var reservations = await _reservationService.GetAllAsync();
        return View(reservations);
    }

    public async Task<IActionResult> Details(int id)
    {
        var reservation = await _reservationService.GetByIdAsync(id);
        return reservation is null ? NotFound() : View(reservation);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Reservation
        {
            ReservationDate = DateTime.Today,
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(9, 0, 0),
            NumberOfAttendees = 1
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Reservation reservation)
    {
        if (!ModelState.IsValid)
            return View(reservation);

        var error = await _reservationService.CreateAsync(reservation);
        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(reservation);
        }

        TempData["Success"] = "Your reservation request has been submitted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var reservation = await _reservationService.GetByIdAsync(id);
        if (reservation is null)
            return NotFound();

        if (reservation.Status != ReservationStatus.Pending)
            return BadRequest("Only pending reservations can be edited.");

        return View(reservation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Reservation reservation)
    {
        if (id != reservation.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(reservation);

        var error = await _reservationService.UpdateAsync(reservation);
        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(reservation);
        }

        TempData["Success"] = "Reservation updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var reservation = await _reservationService.GetByIdAsync(id);
        return reservation is null ? NotFound() : View(reservation);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleted = await _reservationService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        TempData["Success"] = "Reservation deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var error = await _reservationService.ApproveAsync(id);
        if (error is not null)
            TempData["Error"] = error;
        else
            TempData["Success"] = "Reservation approved.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var rejected = await _reservationService.RejectAsync(id);
        TempData[rejected ? "Success" : "Error"] =
            rejected ? "Reservation rejected." : "The reservation could not be rejected.";

        return RedirectToAction(nameof(Index));
    }
}
