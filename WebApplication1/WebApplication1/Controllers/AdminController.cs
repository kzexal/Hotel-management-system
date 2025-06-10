using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using System.Data.Entity;

namespace WebApplication1.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // GET: Admin
        public ActionResult AdminDashBoard()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(Session["UserId"]);
            var user = db.Logins.FirstOrDefault(u => u.LoginId == userId);

            if (user == null || user.TypeAccount != 1)
            {
                return RedirectToAction("Index", "Home");
            }

            // Lấy dữ liệu RoomBooked
            var roomBookings = db.RoomBooked
                .Include(rb => rb.Room)
                .Include(rb => rb.Room.RoomType)
                .Include(rb => rb.Booking)
                .Include(rb => rb.Booking.Guest)
                .OrderByDescending(rb => rb.Booking.CheckInDate)
                .ToList();

            // Lấy dữ liệu Service và Login
            ViewBag.Services = db.Services.ToList();
            ViewBag.Users = db.Logins.Where(u => u.TypeAccount == 0).ToList();
            ViewBag.RoomType = db.RoomTypes.ToList();
            ViewBag.Rooms = db.Rooms.ToList();
            // Tính toán các thống kê
            ViewBag.TotalRooms = db.Rooms.Count();
            ViewBag.TotalBookings = db.Bookings.Count();
            ViewBag.TotalServices = db.Services.Count();
            ViewBag.TotalUsers = db.Logins.Count(u => u.TypeAccount == 0);

            return View(roomBookings);
        }

        // POST: Room/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRoom(Room room)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Kiểm tra xem số phòng đã tồn tại chưa
                    if (db.Rooms.Any(r => r.RoomNumber == room.RoomNumber))
                    {
                        TempData["Error"] = "Room number already exists.";
                    }
                    else
                    {
                        db.Rooms.Add(room);
                        db.SaveChanges();
                        TempData["Success"] = "Room added successfully!";
                        return RedirectToAction("AdminDashBoard");
                    }
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    TempData["Error"] = string.Join(", ", errors);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while adding the room: " + ex.Message;
            }

            // Nếu có lỗi, load lại dữ liệu cần thiết và trả về view
            var roomBookings = db.RoomBooked
                .Include(rb => rb.Room)
                .Include(rb => rb.Room.RoomType)
                .Include(rb => rb.Booking)
                .Include(rb => rb.Booking.Guest)
                .OrderByDescending(rb => rb.Booking.CheckInDate)
                .ToList();

            ViewBag.Services = db.Services.ToList();
            ViewBag.Users = db.Logins.Where(u => u.TypeAccount == 0).ToList();
            ViewBag.RoomType = db.RoomTypes.ToList();
            ViewBag.TotalRooms = db.Rooms.Count();
            ViewBag.TotalBookings = db.Bookings.Count();
            ViewBag.TotalServices = db.Services.Count();
            ViewBag.TotalUsers = db.Logins.Count(u => u.TypeAccount == 0);

            return View("AdminDashBoard", roomBookings);
        }

        // POST: Admin/CreateService
        [HttpPost]
        public ActionResult CreateService(Service service)
        {
            if (ModelState.IsValid)
            {
                db.Services.Add(service);
                db.SaveChanges();
                return RedirectToAction("AdminDashBoard");
            }
            return View(service);
        }

        // POST: Admin/EditService
        [HttpPost]
        public ActionResult EditService(Service service)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(service).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Error updating service: " + ex.Message });
                }
            }
            return Json(new { success = false, message = "Invalid model state" });
        }

        // POST: Admin/DeleteService
        [HttpPost]
        public ActionResult DeleteService(int id)
        {
            Service service = db.Services.Find(id);
            if (service != null)
            {
                db.Services.Remove(service);
                db.SaveChanges();
            }
            return RedirectToAction("AdminDashBoard");
        }

        // GET: Admin/GetAvailableRooms
        public JsonResult GetAvailableRooms(int roomTypeId, DateTime checkInDate, DateTime checkOutDate)
        {
            var availableRooms = db.Rooms
                .Where(r => r.RoomTypeId == roomTypeId)
                .Where(r => !db.RoomBooked.Any(rb =>
                    rb.RoomId == r.RoomId &&
                    ((rb.Booking.CheckInDate <= checkInDate && rb.Booking.CheckOutDate > checkInDate) ||
                     (rb.Booking.CheckInDate < checkOutDate && rb.Booking.CheckOutDate >= checkOutDate) ||
                     (rb.Booking.CheckInDate >= checkInDate && rb.Booking.CheckOutDate <= checkOutDate))))
                .Select(r => new { r.RoomId, r.RoomNumber })
                .ToList();

            return Json(availableRooms, JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/GetRoomTypePrice
        public JsonResult GetRoomTypePrice(int roomTypeId)
        {
            var price = db.RoomTypes
                .Where(rt => rt.RoomTypeId == roomTypeId)
                .Select(rt => rt.Cost)
                .FirstOrDefault();

            return Json(price, JsonRequestBehavior.AllowGet);
        }

        // POST: Admin/CreateBooking
        [HttpPost]
        public ActionResult CreateBooking(Guest guest, DateTime CheckInDate, DateTime CheckOutDate,
            int RoomId, decimal TotalAmount, int[] SelectedServices)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Add guest
                    db.Guests.Add(guest);
                    db.SaveChanges();

                    // Create booking
                    var booking = new Booking
                    {
                        GuestId = guest.GuestId,
                        CheckInDate = CheckInDate,
                        CheckOutDate = CheckOutDate,
                        BookingAmount = (int)TotalAmount,
                        Status = "Confirmed",
                        BookingDate = DateTime.Now
                    };
                    db.Bookings.Add(booking);
                    db.SaveChanges();

                    // Create room booking
                    var roomBooked = new RoomBooked
                    {
                        BookingId = booking.BookingId,
                        RoomId = RoomId
                    };
                    db.RoomBooked.Add(roomBooked);

                    // Add selected services
                    if (SelectedServices != null)
                    {
                        foreach (var serviceId in SelectedServices)
                        {
                            var serviceUsed = new ServicesUsed
                            {
                                BookingId = booking.BookingId,
                                ServiceId = serviceId
                            };
                            db.ServicesUsed.Add(serviceUsed);
                        }
                    }

                    db.SaveChanges();
                    transaction.Commit();
                    return RedirectToAction("AdminDashBoard");
                }
                catch
                {
                    transaction.Rollback();
                    return RedirectToAction("AdminDashBoard");
                }
            }
        }

        // GET: Admin/GetBookingDetails
        public JsonResult GetBookingDetails(int id)
        {
            try
            {
                var roomBooked = db.RoomBooked
                    .Include(rb => rb.Room)
                    .Include(rb => rb.Room.RoomType)
                    .Include(rb => rb.Booking)
                    .Include(rb => rb.Booking.Guest)
                    .Include(rb => rb.Booking.ServicesUsed)
                    .FirstOrDefault(rb => rb.Booking.BookingId == id);

                if (roomBooked == null)
                {
                    return Json(new { success = false, message = "Booking not found" }, JsonRequestBehavior.AllowGet);
                }

                var result = new
                {
                    success = true,
                    booking = new
                    {
                        roomBooked.Booking.BookingId,
                        BookingDate = roomBooked.Booking.BookingDate.ToString("yyyy-MM-dd"),
                        CheckInDate = roomBooked.Booking.CheckInDate.ToString("yyyy-MM-dd"),
                        CheckOutDate = roomBooked.Booking.CheckOutDate.ToString("yyyy-MM-dd"),
                        roomBooked.Booking.BookingAmount,
                        roomBooked.Booking.Status
                    },
                    guest = new
                    {
                        roomBooked.Booking.Guest.GuestFirstName,
                        roomBooked.Booking.Guest.GuestLastName,
                        roomBooked.Booking.Guest.GuestEmailAddress,
                        roomBooked.Booking.Guest.GuestContactNumber
                    },
                    room = new
                    {
                        roomBooked.Room.RoomNumber,
                        RoomType = new
                        {
                            roomBooked.Room.RoomType.Name,
                            roomBooked.Room.RoomType.Cost
                        }
                    },
                    services = roomBooked.Booking.ServicesUsed
                .Where(su => su.Service != null)
                .Select(su => new
                {
                    ServiceName = su.Service.ServiceName,
                    Price = su.Service.ServiceCost
                }).ToList()
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Admin/CancelBooking
        [HttpPost]
        public JsonResult CancelBooking(int id)
        {
            try
            {
                var booking = db.Bookings.Find(id);
                if (booking != null)
                {
                    booking.Status = "Cancelled";
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Booking not found" });
            }
            catch (Exception ex)

            {
                return Json(new { success = false, message = "Error cancelling booking: " + ex.Message });
            }
        }

        // GET: Admin/GetService
        public JsonResult GetService(int id)
        {
            var service = db.Services.Find(id);
            if (service == null)
            {
                return Json(new { success = false, message = "Service not found" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                success = true,
                service = new
                {
                    service.ServiceId,
                    service.ServiceName,
                    service.ServiceDescription,
                    service.ServiceCost
                }
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/GetRoom
        public JsonResult GetRoom(int id)
        {
            var room = db.Rooms.Include(r => r.RoomType).FirstOrDefault(r => r.RoomId == id);
            if (room == null)
            {
                return Json(new { success = false, message = "Room not found" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                success = true,
                room = new
                {
                    room.RoomId,
                    room.RoomNumber,
                    room.RoomTypeId,
                    RoomTypeName = room.RoomType.Name,
                    room.Image,
                    room.Available
                }
            }, JsonRequestBehavior.AllowGet);
        }

        // POST: Admin/EditRoom
        [HttpPost]
        public ActionResult EditRoom(Room room)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(room).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Error updating room: " + ex.Message });
                }
            }
            return Json(new { success = false, message = "Invalid model state" });
        }
    }
}