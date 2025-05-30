using WebApplication1.Models;

public class RoomBooked
{
    public int RoomBookedId { get; set; }
    public int BookingId { get; set; }
    public int RoomId { get; set; }

    public Booking Booking { get; set; }
    public Room Room { get; set; }
}
