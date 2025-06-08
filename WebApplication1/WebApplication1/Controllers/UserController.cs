using System;
using System.Collections.Generic;
using System.Data.Entity;
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
            var user = db.Logins.FirstOrDefault(u => u.LoginId == userId);

            if (user == null || user.TypeAccount != 0)
            {
                return RedirectToAction("Index", "Home");
            }

            var roomBookings = db.RoomBooked
                .Include(rb => rb.Room.RoomType)
                .Include(rb => rb.Booking.Guest)
                .Where(rb => rb.Booking.Guest.UserId == userId)
                .ToList();
            foreach (var rb in roomBookings)
            {
                if (rb.Booking.Status != "Checkout" && rb.Booking.CheckOutDate < DateTime.Today)
                {
                    rb.Booking.Status = "Checkout";
                }
            }
            db.SaveChanges();
            roomBookings = roomBookings
        .OrderBy(rb => rb.Booking.Status == "Checkout" ? 1 : 0)
        .ThenBy(rb => rb.Booking.CheckInDate)
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