using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using WebApplication1.Models;
[Table("Booking", Schema = "Bookings")]
public class Booking
{
    [Key]
    public int BookingId { get; set; }

    [Required]
    public DateTime BookingDate { get; set; }

    [Required]
    public DateTime CheckInDate { get; set; }

    [Required]
    public DateTime CheckOutDate { get; set; }

    [Required]
    public int BookingAmount { get; set; }

    public string GuestId { get; set; }

    [StringLength(20)]
    public string Status { get; set; }

    // Navigation properties
    [ForeignKey("GuestId")]
    public virtual Guest Guest { get; set; }

    // Mối quan hệ 1 booking - nhiều phòng (RoomBooked)
    public virtual ICollection<WebApplication1.Models.RoomBooked> RoomBookings { get; set; }
    public virtual ICollection<ServicesUsed> ServicesUsed { get; set; }
}
