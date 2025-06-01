using System;
using System.Linq;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        public ActionResult Dashboard()
        {
            var today = DateTime.Today;

            // Overview Statistics
            ViewBag.TodayCheckIns = db.Bookings.Count(b => b.CheckInDate.Date == today);
            ViewBag.TodayCheckOuts = db.Bookings.Count(b => b.CheckOutDate.Date == today);
            ViewBag.InHotel = db.Bookings.Count(b => b.CheckInDate.Date <= today && b.CheckOutDate.Date >= today);
            ViewBag.AvailableRooms = db.Rooms.Count(r => r.Available == "Yes");
            ViewBag.OccupiedRooms = db.Rooms.Count(r => r.Available == "No");

            // Room Status
            var roomStatus = new
            {
                Occupied = new
                {
                    Clean = db.Rooms.Count(r => r.Available == "No" && r.Status == "Clean"),
                    Dirty = db.Rooms.Count(r => r.Available == "No" && r.Status == "Dirty"),
                    Inspected = db.Rooms.Count(r => r.Available == "No" && r.Status == "Inspected")
                },
                Available = new
                {
                    Clean = db.Rooms.Count(r => r.Available == "Yes" && r.Status == "Clean"),
                    Dirty = db.Rooms.Count(r => r.Available == "Yes" && r.Status == "Dirty"),
                    Inspected = db.Rooms.Count(r => r.Available == "Yes" && r.Status == "Inspected")
                }
            };
            ViewBag.RoomStatus = roomStatus;

            // Room Types Summary
            var roomTypes = db.RoomTypes.Select(rt => new
            {
                rt.Name,
                Available = db.Rooms.Count(r => r.RoomTypeId == rt.RoomTypeId && r.Available == "Yes"),
                Total = db.Rooms.Count(r => r.RoomTypeId == rt.RoomTypeId),
                rt.BasePrice
            }).ToList();
            ViewBag.RoomTypes = roomTypes;

            // Recent Feedback
            var recentFeedback = db.Feedbacks
                .OrderByDescending(f => f.CreatedAt)
                .Take(3)
                .Select(f => new
                {
                    f.Guest.GuestFirstName,
                    f.Comment,
                    RoomNumber = f.Booking.RoomBooked.FirstOrDefault().Room.RoomNumber
                })
                .ToList();
            ViewBag.RecentFeedback = recentFeedback;

            // Monthly Occupancy
            var monthlyOccupancy = Enumerable.Range(0, 10)
                .Select(i => today.AddMonths(-i))
                .Select(month => new
                {
                    Month = month.ToString("MMM"),
                    Rate = CalculateOccupancyRate(month)
                })
                .Reverse()
                .ToList();
            ViewBag.MonthlyOccupancy = monthlyOccupancy;

            return View();
        }

        private double CalculateOccupancyRate(DateTime month)
        {
            var totalRooms = db.Rooms.Count();
            var startDate = new DateTime(month.Year, month.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var occupiedRoomDays = db.Bookings
                .Where(b => b.CheckInDate <= endDate && b.CheckOutDate >= startDate)
                .Sum(b => (Math.Min(b.CheckOutDate, endDate) - Math.Max(b.CheckInDate, startDate)).Days + 1);

            var totalDays = DateTime.DaysInMonth(month.Year, month.Month);
            var occupancyRate = (double)occupiedRoomDays / (totalRooms * totalDays) * 100;

            return Math.Round(occupancyRate, 2);
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