using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext db;

        public HomeController()
        {
            db = new AppDbContext();
        }

        public ActionResult Index()
        {
            
            UpdateRoomStatusByToday();

            var rooms = db.Rooms.ToList();

            return View(rooms);
        }

     
        private void UpdateRoomStatusByToday()
        {
            try
            {
                var today = DateTime.Today;
                var yesterday = today.AddDays(-1);


                var roomsToSetNo = db.RoomBooked
                    .Include(rb => rb.Booking)
                    .Where(rb => DbFunctions.TruncateTime(rb.Booking.CheckInDate) == today)
                    .Select(rb => rb.RoomId)
                    .Distinct()
                    .ToList();

                foreach (var roomId in roomsToSetNo)
                {
                    var room = db.Rooms.Find(roomId);
                    if (room != null && room.Available != "No")
                    {
                        room.Available = "No";
                    }
                }

 
                var roomsToSetYes = db.RoomBooked
                    .Include(rb => rb.Booking)
                    .Where(rb => DbFunctions.TruncateTime(rb.Booking.CheckOutDate) == yesterday)
                    .Select(rb => rb.RoomId)
                    .Distinct()
                    .ToList();

                foreach (var roomId in roomsToSetYes)
                {
                    var room = db.Rooms.Find(roomId);
                    if (room != null && room.Available != "Yes")
                    {
                        room.Available = "Yes";
                    }
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
             
                System.Diagnostics.Debug.WriteLine("Error in UpdateRoomStatusByToday: " + ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
