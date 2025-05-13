using System;
using WebApplication1.Models;

public class Booking
{
    public int BookingId { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal BookingAmount { get; set; }

    public int GuestId { get; set; }          // UserId (from UserController)
    public int RoomId { get; set; }           // RoomId (from RoomController)
    public string Status { get; set; }

    // Optional: Navigation properties
    public virtual LoginViewModel Guest { get; set; }
    public virtual Room Room { get; set; }
}
