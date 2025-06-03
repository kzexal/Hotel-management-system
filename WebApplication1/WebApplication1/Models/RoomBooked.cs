using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models;

namespace WebApplication1.Models
{
    [Table("RoomBooked", Schema = "Rooms")]
    public class RoomBooked
    {
        public int RoomBookedId { get; set; }
        public int BookingId { get; set; }
        public int RoomId { get; set; }

        public Booking Booking { get; set; }
        public Room Room { get; set; }
    }
}
