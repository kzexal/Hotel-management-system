using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("Payment", Schema = "Bookings")]
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        [StringLength(6)]
        public string PaymentStatus { get; set; } // e.g. "Success", "Failed"

        [Required]
        [StringLength(50)]
        public string PaymentType { get; set; } // e.g. "VNPay", "Momo"

        [Required]
        public int PaymentAmount { get; set; }

        [ForeignKey("Booking")]
        public int BookingId { get; set; }

        public virtual Booking Booking { get; set; }
    }
}
