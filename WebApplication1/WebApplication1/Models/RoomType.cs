using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    [Table("RoomType", Schema = "Rooms")]
    public class RoomType
    {
        [Key]
        public int RoomTypeId { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }

        public virtual ICollection<Room> Rooms { get; set; }
    }

}