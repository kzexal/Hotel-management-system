using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("Room", Schema = "Rooms")]
    public class Room
    {
        [Key]
        public int RoomId { get; set; }

        [Required]
        public string RoomNumber { get; set; }

        [ForeignKey("RoomType")]
        public int RoomTypeId { get; set; }

        [Required]
        [StringLength(5)]
        public string Available { get; set; } 

        [Required]
        [StringLength(255)]
        public string Image { get; set; }

        // Điều hướng
        public virtual RoomType RoomType { get; set; }
    }
}
