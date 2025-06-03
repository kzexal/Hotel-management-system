using System;
using System.Collections.Generic;
using System.Data.Entity; // EF6
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        public ActionResult UserDashboard()
        {
        
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

           
            var roomBookings = db.RoomBooked
                .Include(rb => rb.Room.RoomType)
                .Include(rb => rb.Booking.Guest)
                .Where(rb => rb.Booking.Guest.UserId == userId)
                .ToList();

            return View(roomBookings); 
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
