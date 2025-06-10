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

            // Update booking status for past checkouts
            var pastBookings = db.Bookings
                .Where(b => b.Status != "Checkout" && b.CheckOutDate < DateTime.Today)
                .ToList();

            foreach (var booking in pastBookings)
            {
                booking.Status = "Checkout";
            }
            db.SaveChanges();

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
            ViewBag.Rooms = db.Rooms.Include(r => r.RoomType).ToList();
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
            if (ModelState.IsValid)
            {
                bool roomExists = db.Rooms.Any(r => r.RoomNumber == room.RoomNumber);
                if (roomExists)
                {
                    TempData["Error"] = "Room number already exists!";
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
            ViewBag.Rooms = db.Rooms.Include(r => r.RoomType).ToList();

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
                bool serviceExists = db.Services.Any(s => s.ServiceName == service.ServiceName);
                if (serviceExists)
                {
                    TempData["Error"] = "Service name already exists!";
                }
                else
                {
                    db.Services.Add(service);
                    db.SaveChanges();
                    TempData["Success"] = "Service added successfully!";
                }
                return RedirectToAction("AdminDashBoard");
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
            ViewBag.Rooms = db.Rooms.Include(r => r.RoomType).ToList();

            ViewBag.TotalRooms = db.Rooms.Count();
            ViewBag.TotalBookings = db.Bookings.Count();
            ViewBag.TotalServices = db.Services.Count();
            ViewBag.TotalUsers = db.Logins.Count(u => u.TypeAccount == 0);


            return View("AdminDashBoard", roomBookings); ;
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
            try
            {
                var service = db.Services.Find(id);
                if (service == null)
                {
                    return Json(new { success = false, message = "Service not found" });
                }

                // Check if service is being used in any active bookings
                var hasActiveBookings = db.ServicesUsed.Any(su =>
                    su.ServiceId == id &&
                    su.Booking.CheckOutDate >= DateTime.Today &&
                    su.Booking.Status != "Checkout");

                if (hasActiveBookings)
                {
                    return Json(new { success = false, message = "Cannot delete service that is currently in use" });
                }

                // Remove all past service usage records
                var pastServices = db.ServicesUsed.Where(su => su.ServiceId == id).ToList();
                db.ServicesUsed.RemoveRange(pastServices);

                db.Services.Remove(service);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting service: " + ex.Message });
            }
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

        // GET: Admin/GetUnavailableDates
        public JsonResult GetUnavailableDates(int roomId)
        {
            try
            {
                var bookings = db.RoomBooked
                    .Where(rb => rb.RoomId == roomId)
                    .Join(db.Bookings, 
                          rb => rb.BookingId, 
                          b => b.BookingId, 
                          (rb, b) => new { b.CheckInDate, b.CheckOutDate })
                    .ToList();

                var unavailableDates = new HashSet<string>();
                foreach (var booking in bookings)
                {
                    var start = booking.CheckInDate.AddDays(-1);
                    var end = booking.CheckOutDate;

                    for (var date = start; date <= end; date = date.AddDays(1))
                    {
                        unavailableDates.Add(date.ToString("yyyy-MM-dd"));
                    }
                }

                return Json(new { success = true, dates = unavailableDates.ToList() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error getting unavailable dates: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Admin/CreateBooking
        [HttpPost]
        public ActionResult CreateBooking(Guest guest, DateTime CheckInDate, DateTime CheckOutDate,
            int RoomId, decimal TotalAmount, int[] SelectedServices)
        {
            try
            {
                // Validate input parameters
                if (guest == null || string.IsNullOrEmpty(guest.GuestFirstName) ||
                    string.IsNullOrEmpty(guest.GuestLastName) || string.IsNullOrEmpty(guest.GuestEmailAddress))
                {
                    return Json(new { success = false, message = "Guest information is incomplete" });
                }

                if (CheckInDate >= CheckOutDate)
                {
                    return Json(new { success = false, message = "Check-out date must be after check-in date" });
                }

                if (CheckInDate.Date < DateTime.Today)
                {
                    return Json(new { success = false, message = "Check-in date cannot be in the past" });
                }

                // Check if room exists and is available
                var room = db.Rooms.Find(RoomId);
                if (room == null)
                {
                    return Json(new { success = false, message = "Room not found" });
                }

                if (room.Available == "No")
                {
                    return Json(new { success = false, message = "Room is not available" });
                }

                // Check for date conflicts
                var existingBookings = db.RoomBooked
                    .Where(rb => rb.RoomId == RoomId)
                    .Join(db.Bookings,
                        rb => rb.BookingId,
                        b => b.BookingId,
                        (rb, b) => new { b.CheckInDate, b.CheckOutDate, b.Status })
                    .Where(b => b.Status != "Checkout" &&
                               ((CheckInDate >= b.CheckInDate && CheckInDate < b.CheckOutDate) ||
                                (CheckOutDate > b.CheckInDate && CheckOutDate <= b.CheckOutDate) ||
                                (CheckInDate <= b.CheckInDate && CheckOutDate >= b.CheckOutDate)))
                    .Any();

                if (existingBookings)
                {
                    return Json(new { success = false, message = "Selected dates are not available for this room" });
                }

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Check if guest already exists
                        var existingGuest = db.Guests.FirstOrDefault(g =>
                            g.GuestEmailAddress == guest.GuestEmailAddress &&
                            g.GuestContactNumber == guest.GuestContactNumber);

                        if (existingGuest != null)
                        {
                            // Update existing guest information
                            existingGuest.GuestFirstName = guest.GuestFirstName;
                            existingGuest.GuestLastName = guest.GuestLastName;
                            existingGuest.Street = guest.Street;
                            existingGuest.City = guest.City;
                            existingGuest.CCCD = guest.CCCD;
                            guest = existingGuest;
                        }
                        else
                        {
                            // Add new guest
                            db.Guests.Add(guest);
                        }
                        db.SaveChanges();

                        // Create booking
                        var booking = new Booking
                        {
                            GuestId = guest.GuestId,
                            CheckInDate = CheckInDate,
                            CheckOutDate = CheckOutDate,
                            BookingAmount = (int)TotalAmount,
                            Status = "Checkin",
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

                      
                        db.Entry(room).State = System.Data.Entity.EntityState.Modified;

                        // Add selected services
                        if (SelectedServices != null && SelectedServices.Any())
                        {
                            foreach (var serviceId in SelectedServices)
                            {
                                // Verify service exists
                                var service = db.Services.Find(serviceId);
                                if (service != null)
                                {
                                    var serviceUsed = new ServicesUsed
                                    {
                                        BookingId = booking.BookingId,
                                        ServiceId = serviceId,
                                        ServiceBookingDate = DateTime.Now
                                    };
                                    db.ServicesUsed.Add(serviceUsed);
                                }
                            }
                        }

                        db.SaveChanges();
                        transaction.Commit();

                        return Json(new
                        {
                            success = true,
                            message = "Booking created successfully",
                            bookingId = booking.BookingId
                        });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return Json(new
                        {
                            success = false,
                            message = "Error creating booking: " + ex.Message
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Unexpected error: " + ex.Message
                });
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

        // GET: Admin/GetUser
        public JsonResult GetUser(int id)
        {
            var user = db.Logins.Find(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                success = true,
                user = new
                {
                    user.LoginId,
                    user.Username,
                    user.TypeAccount
                }
            }, JsonRequestBehavior.AllowGet);
        }

        // POST: Admin/EditUser
        [HttpPost]
        public ActionResult EditUser(Login user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = db.Logins.Find(user.LoginId);
                    if (existingUser == null)
                    {
                        return Json(new { success = false, message = "User not found" });
                    }

                    // Update basic info
                    existingUser.Username = user.Username;
                    existingUser.TypeAccount = user.TypeAccount;

                    // Handle password update
                    if (!string.IsNullOrEmpty(user.Password))
                    {
                        existingUser.Password = user.Password;
                    }

                    db.SaveChanges();
                    return Json(new { success = true, message = "User updated successfully" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Error updating user: " + ex.Message });
                }
            }
            return Json(new { success = false, message = "Invalid model state" });
        }

        // POST: Admin/CreateUser
        [HttpPost]
        public ActionResult CreateUser(Login user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    bool userExists = db.Logins.Any(u => u.Username == user.Username);
                    if (userExists)
                    {
                        return Json(new { success = false, message = "Username already exists!" });
                    }
                    user.NewUser = "Yes";
                    db.Logins.Add(user);
                    db.SaveChanges();
                    return Json(new { success = true, message = "User created successfully" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Error creating user: " + ex.Message });
                }
            }

            return Json(new { success = false, message = "Invalid model state" });
        }

        // POST: Admin/DeleteUser
        [HttpPost]
        public ActionResult DeleteUser(int id)
        {
            try
            {
                var user = db.Logins.Find(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Check if the user is an admin
                if (user.TypeAccount == 1)
                {
                    return Json(new { success = false, message = "Cannot delete admin account" });
                }

                // Check if user has any bookings
                var guest = db.Guests.FirstOrDefault(g => g.UserId == id);
                if (guest != null)
                {
                    var hasBookings = db.Bookings.Any(b => b.GuestId == guest.GuestId);
                    if (hasBookings)
                    {
                        return Json(new { success = false, message = "Cannot delete user with active bookings" });
                    }
                    // Remove guest information before deleting user
                    db.Guests.Remove(guest);
                }

                db.Logins.Remove(user);
                db.SaveChanges();

                return Json(new { success = true, message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa người dùng: " + ex.Message });
            }
        }

        // POST: Admin/DeleteRoom
        [HttpPost]
        public ActionResult DeleteRoom(int id)
        {
            try
            {
                var room = db.Rooms.Find(id);
                if (room == null)
                {
                    return Json(new { success = false, message = "Room not found" });
                }

                // Check if room has any active bookings
                var hasActiveBookings = db.RoomBooked.Any(rb =>
                    rb.RoomId == id &&
                    rb.Booking.CheckOutDate >= DateTime.Today &&
                    rb.Booking.Status != "Checkout");

                if (hasActiveBookings)
                {
                    return Json(new { success = false, message = "Cannot delete room with active bookings" });
                }

                // Remove all past bookings for this room
                var pastBookings = db.RoomBooked.Where(rb => rb.RoomId == id).ToList();
                db.RoomBooked.RemoveRange(pastBookings);

                db.Rooms.Remove(room);
                db.SaveChanges();

                return Json(new { success = true, message = "Room deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting room: " + ex.Message });
            }
        }

        // POST: Admin/DeleteBooking
        [HttpPost]
        public ActionResult DeleteBooking(int id)
        {
            try
            {
                var booking = db.Bookings.Find(id);
                if (booking == null)
                {
                    return Json(new { success = false, message = "Booking not found" });
                }

                // Check if booking is active
                if (booking.CheckOutDate >= DateTime.Today && booking.Status != "Checkout")
                {
                    return Json(new { success = false, message = "Cannot delete active bookings" });
                }

                // Remove associated records
                var roomBookings = db.RoomBooked.Where(rb => rb.BookingId == id).ToList();
                var servicesUsed = db.ServicesUsed.Where(su => su.BookingId == id).ToList();

                db.RoomBooked.RemoveRange(roomBookings);
                db.ServicesUsed.RemoveRange(servicesUsed);
                db.Bookings.Remove(booking);
                db.SaveChanges();

                return Json(new { success = true, message = "Booking deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting booking: " + ex.Message });
            }
        }

        // POST: Admin/MarkRoomAsCleaned
        [HttpPost]
        public ActionResult MarkRoomAsCleaned(int id)
        {
            try
            {
                var room = db.Rooms.Find(id);
                if (room == null)
                {
                    return Json(new { success = false, message = "Room not found" });
                }

                // Get the latest booking for this room
                var latestBooking = db.RoomBooked
                    .Include(rb => rb.Booking)
                    .Where(rb => rb.RoomId == id)
                    .OrderByDescending(rb => rb.Booking.CheckOutDate)
                    .FirstOrDefault();

                if (latestBooking == null || latestBooking.Booking.Status != "Checkout")
                {
                    return Json(new { success = false, message = "Room is not marked for cleaning" });
                }

                // Mark room as available
                room.Available = "Yes";
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating room status: " + ex.Message });
            }
        }

        // GET: Admin/GetReportData
        public JsonResult GetReportData(int days = 30)
        {
            try
            {
                var endDate = DateTime.Today;
                var startDate = endDate.AddDays(-days);
                var previousStartDate = startDate.AddDays(-days); // For comparison

                // Get bookings for current period
                var currentBookings = db.Bookings
                    .Where(b => b.BookingDate >= startDate && b.BookingDate <= endDate)
                    .ToList();

                // Get bookings for previous period
                var previousBookings = db.Bookings
                    .Where(b => b.BookingDate >= previousStartDate && b.BookingDate < startDate)
                    .ToList();

                // Calculate daily data
                var dailyData = Enumerable.Range(0, days)
                    .Select(offset => startDate.AddDays(offset))
                    .Select(date => new
                    {
                        Date = date.ToString("MMM dd"),
                        Revenue = currentBookings
                            .Where(b => b.BookingDate.Date == date.Date)
                            .Sum(b => b.BookingAmount),
                        BookingCount = currentBookings
                            .Count(b => b.BookingDate.Date == date.Date)
                    })
                    .ToList();

                // Calculate monthly data
                var monthlyData = currentBookings
                    .GroupBy(b => new { b.BookingDate.Year, b.BookingDate.Month })
                    .Select(g => new
                    {
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                        Revenue = g.Sum(b => b.BookingAmount),
                        BookingCount = g.Count()
                    })
                    .ToList();

                // Calculate summary statistics
                var currentRevenue = currentBookings.Sum(b => b.BookingAmount);
                var previousRevenue = previousBookings.Sum(b => b.BookingAmount);
                var currentBookingCount = currentBookings.Count;
                var previousBookingCount = previousBookings.Count;

                // Calculate average daily rate
                var currentADR = currentBookingCount > 0 ? currentRevenue / currentBookingCount : 0;
                var previousADR = previousBookingCount > 0 ? previousRevenue / previousBookingCount : 0;

                // Calculate occupancy rate
                var totalRooms = db.Rooms.Count();
                var currentOccupancy = (double)currentBookingCount / (totalRooms * days) * 100;
                var previousOccupancy = (double)previousBookingCount / (totalRooms * days) * 100;

                // Calculate percentage changes
                var revenueChange = previousRevenue > 0 ? ((currentRevenue - previousRevenue) / previousRevenue * 100) : 0;
                var bookingsChange = previousBookingCount > 0 ? ((currentBookingCount - previousBookingCount) / (double)previousBookingCount * 100) : 0;
                var rateChange = previousADR > 0 ? ((currentADR - previousADR) / previousADR * 100) : 0;
                var occupancyChange = previousOccupancy > 0 ? (currentOccupancy - previousOccupancy) : 0;

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        labels = dailyData.Select(d => d.Date).ToList(),
                        revenue = dailyData.Select(d => d.Revenue).ToList(),
                        bookings = dailyData.Select(d => d.BookingCount).ToList(),
                        monthlyData = monthlyData
                    },
                    summary = new
                    {
                        totalRevenue = currentRevenue,
                        totalBookings = currentBookingCount,
                        avgDailyRate = currentADR,
                        occupancyRate = currentOccupancy,
                        revenueChange,
                        bookingsChange,
                        rateChange,
                        occupancyChange
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Admin/GetMonthlyRevenue
        public JsonResult GetMonthlyRevenue(int months = 12)
        {
            try
            {
                var endDate = DateTime.Today;
                var startDate = endDate.AddMonths(-months);

                // Get all bookings and services within date range
                var bookings = db.RoomBooked
                    .Include(rb => rb.Booking)
                    .Where(rb => rb.Booking.BookingDate >= startDate && rb.Booking.BookingDate <= endDate)
                    .ToList();

                var services = db.ServicesUsed
                    .Include(su => su.Booking)
                    .Include(su => su.Service)
                    .Where(su => su.Booking.BookingDate >= startDate && su.Booking.BookingDate <= endDate)
                    .ToList();

                // Group data by month after retrieving from database
                var roomRevenue = bookings
                    .GroupBy(rb => new { rb.Booking.BookingDate.Year, rb.Booking.BookingDate.Month })
                    .Select(g => new
                    {
                        Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                        Revenue = g.Sum(rb => rb.Booking.BookingAmount)
                    })
                    .OrderBy(x => x.Month)
                    .ToList();

                var serviceRevenue = services
                    .GroupBy(su => new { su.Booking.BookingDate.Year, su.Booking.BookingDate.Month })
                    .Select(g => new
                    {
                        Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                        Revenue = g.Sum(su => su.Service.ServiceCost)
                    })
                    .OrderBy(x => x.Month)
                    .ToList();

                // Generate all months in range
                var allMonths = Enumerable.Range(0, months)
                    .Select(i => endDate.AddMonths(-i))
                    .Select(d => new DateTime(d.Year, d.Month, 1))
                    .OrderBy(d => d)
                    .ToList();

                // Combine room and service revenue
                var monthlyData = allMonths.Select(month => new
                {
                    Month = month,
                    RoomRevenue = roomRevenue.FirstOrDefault(r => r.Month == month)?.Revenue ?? 0,
                    ServiceRevenue = serviceRevenue.FirstOrDefault(s => s.Month == month)?.Revenue ?? 0
                }).ToList();

                // Calculate summary statistics
                var totalRevenue = monthlyData.Sum(x => x.RoomRevenue + x.ServiceRevenue);
                var averageMonthlyRevenue = totalRevenue / months;
                var bestMonth = monthlyData.OrderByDescending(x => x.RoomRevenue + x.ServiceRevenue).First();

                // Calculate booking counts
                var bookingCounts = bookings
                    .GroupBy(rb => new { rb.Booking.BookingDate.Year, rb.Booking.BookingDate.Month })
                    .Select(g => new
                    {
                        Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Month)
                    .ToList();

                var averageBookings = bookingCounts.Any() ? bookingCounts.Average(x => x.Count) : 0;

                // Calculate percentage change from previous period
                var previousStartDate = startDate.AddMonths(-months);
                var previousBookings = db.Bookings
                    .Where(b => b.BookingDate >= previousStartDate && b.BookingDate < startDate)
                    .ToList();
                var previousRevenue = previousBookings.Sum(b => b.BookingAmount);

                var percentageChange = previousRevenue > 0 
                    ? ((totalRevenue - previousRevenue) / previousRevenue * 100) 
                    : 0;

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        labels = monthlyData.Select(x => x.Month.ToString("MMM yyyy")).ToList(),
                        roomRevenue = monthlyData.Select(x => x.RoomRevenue).ToList(),
                        serviceRevenue = monthlyData.Select(x => x.ServiceRevenue).ToList()
                    },
                    summary = new
                    {
                        totalRevenue = totalRevenue,
                        averageMonthlyRevenue = averageMonthlyRevenue,
                        bestMonth = bestMonth.Month.ToString("MMMM yyyy"),
                        bestMonthRevenue = bestMonth.RoomRevenue + bestMonth.ServiceRevenue,
                        averageBookingsPerMonth = averageBookings,
                        percentageChange = percentageChange
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
