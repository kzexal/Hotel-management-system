using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Payment
    {
        public Room Room { get; set; }
        public Booking Booking { get; set; }
    }
}