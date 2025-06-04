using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class ServicesUsed
    {
        [Key]
        public int ServicesUserId { get; set; }
        public int BookingId { get; set; }
        public virtual Booking Booking { get; set; }

        public int ServiceId { get; set; }
        public virtual Service Service { get; set; }
        public DateTime ServiceBookingDate { get; set; }
    }
}