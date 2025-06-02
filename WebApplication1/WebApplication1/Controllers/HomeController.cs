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
            // Cập nhật trạng thái phòng trước khi hiển thị
            UpdateRoomStatusByToday();

            var rooms = db.Rooms
                          .Include(r => r.RoomType)
                          .Where(r => r.Available == "Yes")
                          .Take(15)
                          .ToList();

            return View(rooms);
        }

        /// <summary>
        /// Tự động cập nhật trạng thái phòng mỗi ngày:
        /// - Nếu hôm nay là ngày Check-in: chuyển sang "No"
        /// - Nếu hôm qua là ngày Check-out: chuyển sang "Yes"
        /// </summary>
        private void UpdateRoomStatusByToday()
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);

            // Danh sách phòng cần chuyển sang "No" vì hôm nay có người Check-in
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

            // Danh sách phòng cần chuyển sang "Yes" vì hôm qua đã Check-out
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
