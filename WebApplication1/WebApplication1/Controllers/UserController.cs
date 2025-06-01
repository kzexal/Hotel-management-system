using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        public ActionResult Dashboard()
        {
            var userId = User.Identity.GetUserId();
            var guest = db.Guests.FirstOrDefault(g => g.UserId == userId);

            if (guest == null)
            {
                return RedirectToAction("Create", "Guest");
            }

            // User Info
            ViewBag.UserName = $"{guest.GuestFirstName} {guest.GuestLastName}";
            ViewBag.MemberSince = guest.CreatedAt;

            var today = DateTime.Today;

            // Current/Upcoming Booking
            var currentBooking = db.Bookings
                .Where(b => b.GuestId == guest.GuestId && b.CheckOutDate >= today)
                .OrderBy(b => b.CheckInDate)
                .Select(b => new
                {
                    RoomType = b.RoomBooked.FirstOrDefault().Room.RoomType.Name,
                    RoomNumber = b.RoomBooked.FirstOrDefault().Room.RoomNumber,
                    RoomImage = b.RoomBooked.FirstOrDefault().Room.Image,
                    b.CheckInDate,
                    b.CheckOutDate,
                    GuestCount = b.RoomBooked.Count(),
                    b.Status
                })
                .FirstOrDefault();

            if (currentBooking != null)
            {
                ViewBag.CurrentBooking = new
                {
                    RoomType = currentBooking.RoomType,
                    RoomNumber = currentBooking.RoomNumber,
                    RoomImage = currentBooking.RoomImage,
                    CheckIn = currentBooking.CheckInDate,
                    CheckOut = currentBooking.CheckOutDate,
                    GuestCount = currentBooking.GuestCount
                };
            }

            // Used Services
            var usedServices = db.ServiceUsages
                .Where(su => su.Booking.GuestId == guest.GuestId)
                .OrderByDescending(su => su.UsageDate)
                .Take(5)
                .Select(su => new
                {
                    Name = su.Service.ServiceName,
                    Icon = GetServiceIcon(su.Service.ServiceType),
                    Date = su.UsageDate,
                    Price = su.Service.Price
                })
                .ToList();

            ViewBag.UsedServices = usedServices;
            ViewBag.TotalServicesAmount = usedServices.Sum(s => s.Price);

            // Booking History
            var bookingHistory = db.Bookings
                .Where(b => b.GuestId == guest.GuestId && b.CheckOutDate < today)
                .OrderByDescending(b => b.CheckOutDate)
                .Take(5)
                .Select(b => new
                {
                    RoomType = b.RoomBooked.FirstOrDefault().Room.RoomType.Name,
                    CheckIn = b.CheckInDate,
                    CheckOut = b.CheckOutDate
                })
                .ToList();

            ViewBag.BookingHistory = bookingHistory;

            // Special Offers
            var specialOffers = db.Promotions
                .Where(p => p.ValidUntil >= today)
                .OrderBy(p => p.ValidUntil)
                .Take(3)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Description,
                    DiscountPercentage = p.DiscountAmount,
                    p.ValidUntil
                })
                .ToList();

            ViewBag.SpecialOffers = specialOffers;

            return View();
        }

        private string GetServiceIcon(string serviceType)
        {
            return serviceType?.ToLower() switch
            {
                "restaurant" => "fas fa-utensils",
                "laundry" => "fas fa-tshirt",
                "spa" => "fas fa-spa",
                "gym" => "fas fa-dumbbell",
                "housekeeping" => "fas fa-broom",
                "room_service" => "fas fa-concierge-bell",
                _ => "fas fa-star"
            };
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