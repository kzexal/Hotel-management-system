using System;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

     
        // [1] Hiển thị form thanh toán
        public ActionResult ShowPaymentForm(int roomId, DateTime checkin, DateTime checkout, int totalPrice)
        {
            var room = db.Rooms.Find(roomId);
            if (room == null)
                return RedirectToAction("PaymentFail");

            ViewBag.CheckIn = checkin;
            ViewBag.CheckOut = checkout;
            ViewBag.TotalPrice = totalPrice;

            return View("~/Views/Booking/Payment.cshtml", room); // View dùng @model Room
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessPayment(FormCollection form)
        {
            try
            {
                int roomId = 0;
                int totalPrice = 0;
                DateTime checkinDate = DateTime.MinValue;
                DateTime checkoutDate = DateTime.MinValue;

                if (!int.TryParse(form["roomId"], out roomId) ||
                    !int.TryParse(form["totalPrice"], out totalPrice) ||
                    !DateTime.TryParse(form["checkin"], out checkinDate) ||
                    !DateTime.TryParse(form["checkout"], out checkoutDate))
                {
                    TempData["BookingError"] = "Invalid input data.";
                    // Store room information in TempData
                    TempData["RoomId"] = form["roomId"];
                    TempData["BookingCheckIn"] = checkinDate;
                    TempData["BookingCheckOut"] = checkoutDate;
                    TempData["BookingTotal"] = totalPrice;
                    return RedirectToAction("PaymentFail");
                }

                string paymentType = form["PaymentType"];
                if (string.IsNullOrEmpty(paymentType))
                {
                    TempData["BookingError"] = "Please select a payment method.";
                    return RedirectToAction("PaymentFail");
                }

<<<<<<< HEAD
                // [1] Lưu thông tin Guest
                var guest = new Guest
                {
                    GuestFirstName = form["GuestFirstName"],
                    GuestLastName = form["GuestLastName"],
                    GuestEmailAddress = form["GuestEmailAddress"],
                    GuestContactNumber = form["GuestContactNumber"],
                    Street = form["Street"],
                    City = form["City"],
                    Zip = form["Zip"],
                    Status = "Active"
                };
                db.Guests.Add(guest);
                db.SaveChanges();
=======
                string email = form["GuestEmailAddress"];
                string contact = form["GuestContactNumber"];
                string firstName = form["GuestFirstName"];
                string lastName = form["GuestLastName"];
                string street = form["Street"];
                string city = form["City"];
                string guestId = form["GuestId"];
                
                if (string.IsNullOrEmpty(guestId))
                {
                    TempData["BookingError"] = "CCCD không được để trống.";
                    return RedirectToAction("PaymentFail");
                }

                var guest = db.Guests.FirstOrDefault(g => g.GuestId == guestId);

                if (guest == null)
                {
                    guest = new Guest
                    {
                        GuestId = guestId,
                        GuestFirstName = firstName,
                        GuestLastName = lastName,
                        GuestEmailAddress = email,
                        GuestContactNumber = contact,
                        Street = street,
                        City = city,
                        Status = "Reserved",
                        UserId = Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : (int?)null
                    };
                    db.Guests.Add(guest);
                    db.SaveChanges();
                }
>>>>>>> b8825aeab2dcd453c462c0321de4bc1f713010c5

                // [2] Tạo Booking
                var booking = new Booking
                {
                    GuestId = guest.GuestId,
                    BookingDate = DateTime.Now,
                    CheckInDate = checkinDate,
                    CheckOutDate = checkoutDate,
                    BookingAmount = totalPrice,
                    Status = "Confirmed" // ✅ Đặt luôn là Confirmed
                };
                db.Bookings.Add(booking);
                db.SaveChanges();

                // [3] Đánh dấu Room đã được đặt
                db.RoomBooked.Add(new RoomBooked
                {
                    BookingId = booking.BookingId,
                    RoomId = roomId
                });

                var room = db.Rooms.Find(roomId);
                if (room != null)
                {
                    room.Available = "No";
                }
                db.SaveChanges();

                // [4] Tạo bản ghi Payment (thanh toán giả lập thành công)
                var payment = new Payment
                {
                    BookingId = booking.BookingId,
                    PaymentAmount = totalPrice,
                    PaymentStatus = "Success",
                    PaymentType = paymentType
                };
                db.Payments.Add(payment);
                db.SaveChanges();

                TempData["BookingSuccess"] = $"Đặt phòng và thanh toán thành công! Mã đặt: {booking.BookingId}";
                return RedirectToAction("BookingSuccess", "Booking");
            }
            catch (Exception)
            {
                TempData["BookingError"] = "Đã xảy ra lỗi khi xử lý thanh toán.";
                return RedirectToAction("PaymentFail");
            }
        }

<<<<<<< HEAD
=======
        // Handle service payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessServicePayment(int bookingId, string PaymentType, int[] selectedServiceIds)
        {
            try
            {
                var booking = db.Bookings.FirstOrDefault(b => b.BookingId == bookingId);
                if (booking == null || selectedServiceIds == null || selectedServiceIds.Length == 0)
                {
                    TempData["ServiceError"] = "Booking not found or no services selected.";
                    return RedirectToAction("UserDashboard", "User");
                }

                int totalCost = 0;
                foreach (var serviceId in selectedServiceIds)
                {
                    var service = db.Services.Find(serviceId);
                    if (service != null)
                    {
                        totalCost += service.ServiceCost;

                        db.ServicesUsed.Add(new ServicesUsed
                        {
                            BookingId = bookingId,
                            ServiceId = serviceId,
                            ServiceBookingDate = DateTime.Now
                        });
                        db.SaveChanges();
                    }
                }

                booking.BookingAmount += totalCost;
                db.SaveChanges();

                db.Payments.Add(new Payment
                {
                    BookingId = booking.BookingId,
                    PaymentAmount = totalCost,
                    PaymentStatus = "1",
                    PaymentType = PaymentType
                });
                db.SaveChanges();

                TempData["ServiceSuccess"] = "Service payment was successful.";
            }
            catch (Exception ex)
            {
                string errorMessage = GetFullErrorMessage(ex);
                System.Diagnostics.Debug.WriteLine("Service payment error: " + errorMessage);
                TempData["ServiceError"] = "Error while processing service payment: " + errorMessage;
            }

            return RedirectToAction("UserDashboard", "User");
        }

        // Error page
>>>>>>> b8825aeab2dcd453c462c0321de4bc1f713010c5
        public ActionResult PaymentFail()
        {
            ViewBag.Message = TempData["BookingError"];
            
            // Get room information from TempData
            if (TempData["RoomId"] != null)
            {
                var roomId = Convert.ToInt32(TempData["RoomId"]);
                var room = db.Rooms.Find(roomId);
                return View(room);
            }
            
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
