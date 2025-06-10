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
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);

    
            var roomsToSetNo = db.RoomBooked
                .Join(db.Bookings, rb => rb.BookingId, b => b.BookingId, (rb, b) => new { rb.RoomId, b.CheckInDate })
                .Where(x => DbFunctions.TruncateTime(x.CheckInDate) == today)
                .Select(x => x.RoomId)
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
                .Join(db.Bookings, rb => rb.BookingId, b => b.BookingId, (rb, b) => new { rb.RoomId, b.CheckOutDate })
                .Where(x => DbFunctions.TruncateTime(x.CheckOutDate) == yesterday)
                .Select(x => x.RoomId)
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
