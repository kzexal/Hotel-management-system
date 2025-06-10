using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class BookingController : Controller
    {
        private readonly WebApplicationContext db;

        public BookingController(WebApplicationContext context)
        {
            db = context;
        }

        public IActionResult BookingPage(int roomId)
        {
            var bookings = db.RoomBooked
                .Where(rb => rb.RoomId == roomId)
                .Join(db.Bookings, rb => rb.BookingId, b => b.BookingId, (rb, b) => new { b.CheckInDate, b.CheckOutDate })
                .ToList();

            var unavailableDates = new HashSet<string>();
            foreach (var booking in bookings)
            {
                var start = booking.CheckInDate;
                var end = booking.CheckOutDate;

                // Ensure we don't exceed DateTime bounds
                if (start <= DateTime.MaxValue && end >= DateTime.MinValue)
                {
                    for (var date = start; date <= end; date = date.AddDays(1))
                    {
                        unavailableDates.Add(date.ToString("yyyy-MM-dd"));
                    }
                }
            }

            ViewBag.UnavailableDates = unavailableDates.ToList();
            ViewBag.RoomId = roomId;
            
            return View("BookingPage");
        }

        [HttpPost]
        public IActionResult BookRoom(int roomId, DateTime checkInDate, DateTime checkOutDate)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Request.RawUrl });
            }

            try
            {
                // Tạo booking mới
                var booking = new Booking
                {
                    UserId = User.Identity.Name, // Hoặc lấy UserId từ claim
                    BookingDate = DateTime.Now,
                    CheckInDate = checkInDate,
                    CheckOutDate = checkOutDate,
                    Status = "Pending"
                };

                db.Bookings.Add(booking);
                db.SaveChanges();

                // Tạo RoomBooked
                var roomBooked = new RoomBooked
                {
                    RoomId = roomId,
                    BookingId = booking.BookingId
                };

                db.RoomBooked.Add(roomBooked);
                db.SaveChanges();

                return RedirectToAction("BookingSuccess");
            }
            catch (Exception ex)
            {
                // Log error
                ModelState.AddModelError("", "Có lỗi xảy ra khi đặt phòng. Vui lòng thử lại sau.");
                return RedirectToAction("BookingPage", new { roomId = roomId });
            }
        }

        public IActionResult BookingSuccess()
        {
            return View();
        }
    }
} 