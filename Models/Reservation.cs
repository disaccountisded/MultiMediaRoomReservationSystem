using System.ComponentModel.DataAnnotations;

namespace MultiMediaRoomReservationSystem.Models;

public class Reservation : BaseEntity
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Reserved by")]
    public string ReservedBy { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Purpose { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Reservation date")]
    public DateTime ReservationDate { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "Start time")]
    public TimeSpan StartTime { get; set; } = new(8, 0, 0);

    [Required]
    [Display(Name = "End time")]
    public TimeSpan EndTime { get; set; } = new(9, 0, 0);

    [Range(1, 100)]
    [Display(Name = "Number of attendees")]
    public int NumberOfAttendees { get; set; } = 1;

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    [Display(Name = "Date submitted")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
