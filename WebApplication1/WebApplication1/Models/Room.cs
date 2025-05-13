using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public decimal PricePerNight { get; set; }
        public decimal PricePerDay { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
    }

}